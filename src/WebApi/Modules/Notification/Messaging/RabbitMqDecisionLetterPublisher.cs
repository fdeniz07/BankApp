using System.Text;
using System.Text.Json;
using Application.Services.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WebApi.Modules.Notification.Messaging;

public sealed class RabbitMqDecisionLetterPublisher : IDecisionLetterPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _rabbitMq;
    private readonly ILogger<RabbitMqDecisionLetterPublisher> _logger;

    public RabbitMqDecisionLetterPublisher(
        IOptions<RabbitMqOptions> rabbitMq,
        ILogger<RabbitMqDecisionLetterPublisher> logger)
    {
        _rabbitMq = rabbitMq.Value;
        _logger = logger;
    }

    public Task PublishAsync(DecisionLetterRequested message, CancellationToken cancellationToken = default)
    {
        if (!_rabbitMq.Enabled)
        {
            _logger.LogInformation(
                "Decision letter (RabbitMQ disabled). To={Email} Decision={Decision} Subject={Subject} Body={Body}",
                message.ToEmail,
                message.Decision,
                message.Subject,
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
                queue: _rabbitMq.DecisionLetterQueue,
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
                routingKey: _rabbitMq.DecisionLetterQueue,
                basicProperties: props,
                body: body);

            _logger.LogInformation(
                "Decision letter queued. To={Email} Decision={Decision} Queue={Queue}",
                message.ToEmail,
                message.Decision,
                _rabbitMq.DecisionLetterQueue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "RabbitMQ publish failed; logging decision letter instead. To={Email} Subject={Subject} Body={Body}",
                message.ToEmail,
                message.Subject,
                message.Body);
        }

        return Task.CompletedTask;
    }
}
