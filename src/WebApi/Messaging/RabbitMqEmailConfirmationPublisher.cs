using System.Text;
using System.Text.Json;
using Application.Services.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WebApi.Messaging;

public sealed class RabbitMqEmailConfirmationPublisher : IEmailConfirmationPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _rabbitMq;
    private readonly AppPublicOptions _app;
    private readonly ILogger<RabbitMqEmailConfirmationPublisher> _logger;

    public RabbitMqEmailConfirmationPublisher(
        IOptions<RabbitMqOptions> rabbitMq,
        IOptions<AppPublicOptions> app,
        ILogger<RabbitMqEmailConfirmationPublisher> logger)
    {
        _rabbitMq = rabbitMq.Value;
        _app = app.Value;
        _logger = logger;
    }

    public Task PublishAsync(EmailConfirmationRequested message, CancellationToken cancellationToken = default)
    {
        string confirmUrl =
            $"{_app.PublicClientBaseUrl.TrimEnd('/')}/confirm-email?token={Uri.EscapeDataString(message.Token)}";

        var payload = new
        {
            message.UserId,
            message.Email,
            message.Token,
            ConfirmUrl = confirmUrl
        };

        if (!_rabbitMq.Enabled)
        {
            _logger.LogInformation(
                "Email confirmation (RabbitMQ disabled). To={Email} Url={ConfirmUrl}",
                message.Email,
                confirmUrl);
            return Task.CompletedTask;
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMq.HostName,
                Port = _rabbitMq.Port,
                UserName = _rabbitMq.UserName,
                Password = _rabbitMq.Password,
                DispatchConsumersAsync = true
            };

            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: _rabbitMq.EmailConfirmationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, JsonOptions));
            IBasicProperties props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.ContentType = "application/json";

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _rabbitMq.EmailConfirmationQueue,
                basicProperties: props,
                body: body);

            _logger.LogInformation(
                "Email confirmation queued. To={Email} Queue={Queue}",
                message.Email,
                _rabbitMq.EmailConfirmationQueue);
        }
        catch (Exception ex)
        {
            // Dev-friendly fallback so registration still succeeds if broker is down.
            _logger.LogWarning(
                ex,
                "RabbitMQ publish failed; logging confirmation link instead. To={Email} Url={ConfirmUrl}",
                message.Email,
                confirmUrl);
        }

        return Task.CompletedTask;
    }
}
