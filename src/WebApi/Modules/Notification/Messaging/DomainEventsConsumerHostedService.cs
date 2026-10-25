using System.Text;
using System.Text.Json;
using Application.Modules.Credit.Sagas;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApi.Modules.Notification.Realtime;

namespace WebApi.Modules.Notification.Messaging;

public sealed class DomainEventsConsumerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DomainEventsConsumerHostedService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public DomainEventsConsumerHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<DomainEventsConsumerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Domain events consumer disabled (RabbitMq:Enabled=false).");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConnectAndConsume();
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                DisposeChannel();
                _logger.LogWarning(ex, "Domain events consumer not connected; retrying in 5s.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private void ConnectAndConsume()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: _options.DomainEventsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnReceivedAsync;

        _channel.BasicConsume(
            queue: _options.DomainEventsQueue,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Listening for domain events on queue {Queue}.",
            _options.DomainEventsQueue);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            string json = Encoding.UTF8.GetString(ea.Body.ToArray());
            using JsonDocument doc = JsonDocument.Parse(json);
            string type = doc.RootElement.TryGetProperty("type", out JsonElement typeEl)
                ? typeEl.GetString() ?? "(unknown)"
                : "(unknown)";
            string payload = doc.RootElement.TryGetProperty("payload", out JsonElement payloadEl)
                ? payloadEl.GetString() ?? "{}"
                : json;

            _logger.LogInformation(
                "Domain event consumed. Type={EventType} Payload={Payload}",
                type,
                payload);

            using IServiceScope scope = _scopeFactory.CreateScope();
            ICreditApplicationProcessManager processManager =
                scope.ServiceProvider.GetRequiredService<ICreditApplicationProcessManager>();
            await processManager.HandleAsync(type, payload);

            CreditApplicationChangedMessage? realtime = TryBuildRealtimeMessage(type, payload);
            if (realtime is not null)
            {
                string? currencyCode = await ResolveCurrencyCodeAsync(
                    scope.ServiceProvider,
                    realtime.ApplicationId);
                realtime = realtime with { CurrencyCode = currencyCode };

                ICreditApplicationRealtimeNotifier notifier =
                    scope.ServiceProvider.GetRequiredService<ICreditApplicationRealtimeNotifier>();
                await notifier.NotifyAsync(realtime);
            }

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process domain event message.");
            _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private static CreditApplicationChangedMessage? TryBuildRealtimeMessage(string eventType, string payload)
    {
        string shortName = ShortEventName(eventType);
        if (shortName is not (
            nameof(CreditApplicationCreated)
            or nameof(CreditApplicationApproved)
            or nameof(CreditApplicationRejected)
            or nameof(CreditApplicationCancelled)))
        {
            return null;
        }

        using JsonDocument doc = JsonDocument.Parse(payload);
        JsonElement root = doc.RootElement;

        if (!TryGetGuid(root, "creditApplicationId", out Guid applicationId)
            || !TryGetGuid(root, "customerId", out Guid customerId))
        {
            return null;
        }

        string applicationNumber = root.TryGetProperty("applicationNumber", out JsonElement numberEl)
            ? numberEl.GetString() ?? string.Empty
            : string.Empty;

        DateTimeOffset occurredOnUtc = DateTimeOffset.UtcNow;
        if (root.TryGetProperty("occurredOnUtc", out JsonElement occurredEl)
            && occurredEl.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(occurredEl.GetString(), out DateTimeOffset parsed))
        {
            occurredOnUtc = parsed;
        }

        decimal? amount = shortName switch
        {
            nameof(CreditApplicationCreated) => TryGetDecimal(root, "requestedAmount"),
            nameof(CreditApplicationApproved) => TryGetDecimal(root, "approvedAmount"),
            _ => null
        };

        return new CreditApplicationChangedMessage(
            applicationId,
            applicationNumber,
            customerId,
            shortName,
            occurredOnUtc,
            amount);
    }

    private static async Task<string?> ResolveCurrencyCodeAsync(
        IServiceProvider services,
        Guid applicationId)
    {
        ICreditApplicationRepository applications =
            services.GetRequiredService<ICreditApplicationRepository>();
        ICreditSubTypeRepository subTypes =
            services.GetRequiredService<ICreditSubTypeRepository>();
        ICreditTypeRepository creditTypes =
            services.GetRequiredService<ICreditTypeRepository>();

        CreditApplication? application = await applications.GetAsync(
            a => a.Id == applicationId);
        if (application is null)
            return null;

        CreditSubType? subType = await subTypes.GetAsync(
            s => s.Id == application.CreditSubTypeId);
        if (subType is null)
            return null;

        CreditType? creditType = await creditTypes.GetAsync(
            t => t.Id == subType.CreditTypeId);
        if (creditType is null || string.IsNullOrWhiteSpace(creditType.CurrencyCode))
            return "USD";

        return creditType.CurrencyCode;
    }

    private static string ShortEventName(string eventType)
    {
        int lastDot = eventType.LastIndexOf('.');
        return lastDot >= 0 && lastDot < eventType.Length - 1
            ? eventType[(lastDot + 1)..]
            : eventType;
    }

    private static bool TryGetGuid(JsonElement root, string name, out Guid value)
    {
        value = default;
        if (!root.TryGetProperty(name, out JsonElement el))
            return false;
        if (el.ValueKind == JsonValueKind.String)
            return Guid.TryParse(el.GetString(), out value);
        return el.TryGetGuid(out value);
    }

    private static decimal? TryGetDecimal(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out JsonElement el))
            return null;
        if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out decimal number))
            return number;
        if (el.ValueKind == JsonValueKind.String
            && decimal.TryParse(el.GetString(), System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal parsed))
            return parsed;
        return null;
    }

    private void DisposeChannel()
    {
        try { _channel?.Dispose(); } catch { /* ignore */ }
        try { _connection?.Dispose(); } catch { /* ignore */ }
        _channel = null;
        _connection = null;
    }

    public override void Dispose()
    {
        DisposeChannel();
        base.Dispose();
    }
}
