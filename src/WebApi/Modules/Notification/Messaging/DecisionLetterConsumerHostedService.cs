using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi.Modules.Notification.Messaging;

/// <summary>
/// Lab consumer: reads decision-letter queue and logs To/Subject/Body to Seq (no SMTP).
/// </summary>
public sealed class DecisionLetterConsumerHostedService : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DecisionLetterConsumerHostedService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public DecisionLetterConsumerHostedService(
        IOptions<RabbitMqOptions> options,
        ILogger<DecisionLetterConsumerHostedService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Decision letter consumer disabled (RabbitMq:Enabled=false).");
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
                _logger.LogWarning(
                    ex,
                    "Decision letter consumer not connected; retrying in 5s.");
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
            queue: _options.DecisionLetterQueue,
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
                JsonElement root = doc.RootElement;

                string to = root.TryGetProperty("toEmail", out JsonElement toEl)
                    ? toEl.GetString() ?? "(unknown)"
                    : "(unknown)";
                string decision = root.TryGetProperty("decision", out JsonElement decisionEl)
                    ? decisionEl.GetString() ?? "(unknown)"
                    : "(unknown)";
                string subject = root.TryGetProperty("subject", out JsonElement subjectEl)
                    ? subjectEl.GetString() ?? "(missing)"
                    : "(missing)";
                string body = root.TryGetProperty("body", out JsonElement bodyEl)
                    ? bodyEl.GetString() ?? "(empty)"
                    : "(empty)";
                string applicationNumber = root.TryGetProperty("applicationNumber", out JsonElement appEl)
                    ? appEl.GetString() ?? ""
                    : "";

                _logger.LogInformation(
                    "Decision letter consumed. To={Email} Decision={Decision} Application={ApplicationNumber} Subject={Subject} Body={Body}",
                    to,
                    decision,
                    applicationNumber,
                    subject,
                    body);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process decision letter message.");
                _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }

            await Task.CompletedTask;
        };

        _channel.BasicConsume(
            queue: _options.DecisionLetterQueue,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Listening for decision letters on queue {Queue}.",
            _options.DecisionLetterQueue);
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
