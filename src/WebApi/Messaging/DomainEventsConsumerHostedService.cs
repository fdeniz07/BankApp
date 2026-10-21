using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi.Messaging;

public sealed class DomainEventsConsumerHostedService : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DomainEventsConsumerHostedService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public DomainEventsConsumerHostedService(
        IOptions<RabbitMqOptions> options,
        ILogger<DomainEventsConsumerHostedService> logger)
    {
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
        consumer.Received += async (_, ea) =>
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

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process domain event message.");
                _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }

            await Task.CompletedTask;
        };

        _channel.BasicConsume(
            queue: _options.DomainEventsQueue,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Listening for domain events on queue {Queue}.",
            _options.DomainEventsQueue);
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
