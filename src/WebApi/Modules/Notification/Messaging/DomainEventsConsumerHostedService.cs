using System.Text;
using System.Text.Json;
using Application.Modules.Credit.Sagas;
using Application.Services.Messaging;
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

            await TryPublishDecisionLetterAsync(scope.ServiceProvider, type, payload);
            await TryPublishDecisionSmsAsync(scope.ServiceProvider, type, payload);

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

    private async Task TryPublishDecisionLetterAsync(
        IServiceProvider services,
        string eventType,
        string payload)
    {
        string shortName = ShortEventName(eventType);
        if (shortName is not (nameof(CreditApplicationApproved) or nameof(CreditApplicationRejected)))
            return;

        using JsonDocument doc = JsonDocument.Parse(payload);
        JsonElement root = doc.RootElement;

        if (!TryGetGuid(root, "creditApplicationId", out Guid applicationId)
            || !TryGetGuid(root, "customerId", out Guid customerId))
        {
            _logger.LogWarning(
                "Decision letter skipped: missing application/customer id. Type={EventType}",
                shortName);
            return;
        }

        string applicationNumber = root.TryGetProperty("applicationNumber", out JsonElement numberEl)
            ? numberEl.GetString() ?? string.Empty
            : string.Empty;

        string? rejectionReason = root.TryGetProperty("rejectionReason", out JsonElement reasonEl)
            ? reasonEl.GetString()
            : null;

        DateTimeOffset occurredOnUtc = DateTimeOffset.UtcNow;
        if (root.TryGetProperty("occurredOnUtc", out JsonElement occurredEl)
            && occurredEl.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(occurredEl.GetString(), out DateTimeOffset parsed))
        {
            occurredOnUtc = parsed;
        }

        IApplicationUserRepository users = services.GetRequiredService<IApplicationUserRepository>();
        ApplicationUser? user = await users.GetAsync(u => u.CustomerId == customerId);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogWarning(
                "Decision letter skipped: no customer email for CustomerId={CustomerId} Application={ApplicationNumber}",
                customerId,
                applicationNumber);
            return;
        }

        ICreditApplicationRepository applications =
            services.GetRequiredService<ICreditApplicationRepository>();
        CreditApplication? application = await applications.GetAsync(a => a.Id == applicationId);
        string? notes = application?.Notes;

        bool approved = shortName == nameof(CreditApplicationApproved);
        string decision = approved ? "Approved" : "Rejected";
        string subject = approved
            ? $"BankApp — Application {applicationNumber} approved"
            : $"BankApp — Application {applicationNumber} rejected";

        string body;
        if (!string.IsNullOrWhiteSpace(notes))
            body = notes.Trim();
        else if (!approved && !string.IsNullOrWhiteSpace(rejectionReason))
            body = $"Your credit application {applicationNumber} was rejected. Reason: {rejectionReason}";
        else if (approved)
            body = $"Your credit application {applicationNumber} was approved.";
        else
            body = $"Your credit application {applicationNumber} was rejected.";

        string? customerName = FormatCustomerName(user.FirstName, user.LastName);

        IDecisionLetterPublisher publisher = services.GetRequiredService<IDecisionLetterPublisher>();
        await publisher.PublishAsync(
            new DecisionLetterRequested(
                user.Email,
                customerName,
                applicationNumber,
                decision,
                subject,
                body,
                occurredOnUtc));
    }

    private async Task TryPublishDecisionSmsAsync(
        IServiceProvider services,
        string eventType,
        string payload)
    {
        string shortName = ShortEventName(eventType);
        if (shortName is not (nameof(CreditApplicationApproved) or nameof(CreditApplicationRejected)))
            return;

        using JsonDocument doc = JsonDocument.Parse(payload);
        JsonElement root = doc.RootElement;

        if (!TryGetGuid(root, "creditApplicationId", out Guid applicationId)
            || !TryGetGuid(root, "customerId", out Guid customerId))
        {
            _logger.LogWarning(
                "Decision SMS skipped: missing application/customer id. Type={EventType}",
                shortName);
            return;
        }

        string applicationNumber = root.TryGetProperty("applicationNumber", out JsonElement numberEl)
            ? numberEl.GetString() ?? string.Empty
            : string.Empty;

        string? rejectionReason = root.TryGetProperty("rejectionReason", out JsonElement reasonEl)
            ? reasonEl.GetString()
            : null;

        DateTimeOffset occurredOnUtc = DateTimeOffset.UtcNow;
        if (root.TryGetProperty("occurredOnUtc", out JsonElement occurredEl)
            && occurredEl.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(occurredEl.GetString(), out DateTimeOffset parsed))
        {
            occurredOnUtc = parsed;
        }

        ICustomerRepository customers = services.GetRequiredService<ICustomerRepository>();
        Customer? customer = await customers.GetAsync(c => c.Id == customerId);
        if (customer is null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
        {
            _logger.LogWarning(
                "Decision SMS skipped: no customer phone for CustomerId={CustomerId} Application={ApplicationNumber}",
                customerId,
                applicationNumber);
            return;
        }

        ICreditApplicationRepository applications =
            services.GetRequiredService<ICreditApplicationRepository>();
        CreditApplication? application = await applications.GetAsync(a => a.Id == applicationId);
        string? notes = application?.Notes;

        bool approved = shortName == nameof(CreditApplicationApproved);
        string decision = approved ? "Approved" : "Rejected";

        // SMS body: prefer staff notes, else short status text (keep compact for lab).
        string body;
        if (!string.IsNullOrWhiteSpace(notes))
            body = TruncateSms(notes.Trim());
        else if (!approved && !string.IsNullOrWhiteSpace(rejectionReason))
            body = TruncateSms($"BankApp: {applicationNumber} rejected. {rejectionReason}");
        else if (approved)
            body = TruncateSms($"BankApp: {applicationNumber} approved.");
        else
            body = TruncateSms($"BankApp: {applicationNumber} rejected.");

        IApplicationUserRepository users = services.GetRequiredService<IApplicationUserRepository>();
        ApplicationUser? user = await users.GetAsync(u => u.CustomerId == customerId);
        string? customerName = user is null ? null : FormatCustomerName(user.FirstName, user.LastName);

        IDecisionSmsPublisher publisher = services.GetRequiredService<IDecisionSmsPublisher>();
        await publisher.PublishAsync(
            new DecisionSmsRequested(
                customer.PhoneNumber.Trim(),
                customerName,
                applicationNumber,
                decision,
                body,
                occurredOnUtc));
    }

    private static string TruncateSms(string text, int maxLength = 320)
    {
        if (text.Length <= maxLength)
            return text;
        return text[..(maxLength - 1)] + "…";
    }

    private static string? FormatCustomerName(string? firstName, string? lastName)
    {
        string name = $"{firstName} {lastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? null : name;
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
