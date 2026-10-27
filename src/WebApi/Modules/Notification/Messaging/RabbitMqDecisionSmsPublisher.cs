using System.Text;
using System.Text.Json;
using Application.Services.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WebApi.Modules.Notification.Messaging;

public sealed class RabbitMqDecisionSmsPublisher : IDecisionSmsPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _rabbitMq;
    private readonly ILogger<RabbitMqDecisionSmsPublisher> _logger;

    public RabbitMqDecisionSmsPublisher(
        IOptions<RabbitMqOptions> rabbitMq,
        ILogger<RabbitMqDecisionSmsPublisher> logger)
    {
        _rabbitMq = rabbitMq.Value;
        _logger = logger;
    }

    public Task PublishAsync(DecisionSmsRequested message, CancellationToken cancellationToken = default)
    {
        if (!_rabbitMq.Enabled)
        {
            _logger.LogInformation(
                "Decision SMS (RabbitMQ disabled). To={Phone} Decision={Decision} Body={Body}",
                message.ToPhone,
                message.Decision,
                message.Body);
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
                queue: _rabbitMq.DecisionSmsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOptions));
            IBasicProperties props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.ContentType = "application/json";

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _rabbitMq.DecisionSmsQueue,
                basicProperties: props,
                body: body);

            _logger.LogInformation(
                "Decision SMS queued. To={Phone} Decision={Decision} Queue={Queue}",
                message.ToPhone,
                message.Decision,
                _rabbitMq.DecisionSmsQueue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "RabbitMQ publish failed; logging decision SMS instead. To={Phone} Body={Body}",
                message.ToPhone,
                message.Body);
        }

        return Task.CompletedTask;
    }
}
