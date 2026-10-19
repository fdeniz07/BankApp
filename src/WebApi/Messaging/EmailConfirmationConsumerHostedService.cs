using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi.Messaging;

public sealed class EmailConfirmationConsumerHostedService : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<EmailConfirmationConsumerHostedService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public EmailConfirmationConsumerHostedService(
        IOptions<RabbitMqOptions> options,
        ILogger<EmailConfirmationConsumerHostedService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email confirmation consumer disabled (RabbitMq:Enabled=false).");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConnectAndConsume();
                // Keep the hosted service alive until shutdown; channel callbacks do the work.
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
                    "Email confirmation consumer not connected; retrying in 5s. Publisher fallback logs still apply.");
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
            queue: _options.EmailConfirmationQueue,
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
                string email = doc.RootElement.GetProperty("email").GetString() ?? "(unknown)";
                string confirmUrl = doc.RootElement.GetProperty("confirmUrl").GetString() ?? "(missing)";

                _logger.LogInformation(
                    "Email confirmation message consumed. To={Email} Url={ConfirmUrl}",
                    email,
                    confirmUrl);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email confirmation message.");
                _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }

            await Task.CompletedTask;
        };

        _channel.BasicConsume(
            queue: _options.EmailConfirmationQueue,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Listening for email confirmation messages on queue {Queue}.",
            _options.EmailConfirmationQueue);
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
