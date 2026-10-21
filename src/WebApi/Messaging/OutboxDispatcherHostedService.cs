using System.Text;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Contexts;
using RabbitMQ.Client;

namespace WebApi.Messaging;

public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;

    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Outbox dispatcher idle (RabbitMq:Enabled=false); messages stay in OutboxMessages until enabled.");
            return;
        }

        _logger.LogInformation("Outbox dispatcher started. Queue={Queue}", _options.DomainEventsQueue);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Outbox dispatcher cycle failed; retrying in 5s.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        BankingDbContext db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        List<OutboxMessage> pending = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && !m.IsDeleted)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        IConnection? connection = null;
        IModel? channel = null;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: _options.DomainEventsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            foreach (OutboxMessage message in pending)
            {
                try
                {
                    var envelope = new
                    {
                        message.Id,
                        message.Type,
                        message.Payload,
                        message.OccurredOnUtc
                    };

                    byte[] body = Encoding.UTF8.GetBytes(
                        System.Text.Json.JsonSerializer.Serialize(
                            envelope,
                            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)));

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.ContentType = "application/json";
                    props.Type = message.Type;

                    channel.BasicPublish(
                        exchange: string.Empty,
                        routingKey: _options.DomainEventsQueue,
                        basicProperties: props,
                        body: body);

                    message.ProcessedOnUtc = DateTimeOffset.UtcNow;
                    message.Error = null;
                    message.UpdatedDate = DateTimeOffset.UtcNow;

                    _logger.LogInformation(
                        "Outbox message published. Id={OutboxId} Type={EventType}",
                        message.Id,
                        message.Type);
                }
                catch (Exception ex)
                {
                    message.Error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                    message.UpdatedDate = DateTimeOffset.UtcNow;
                    _logger.LogWarning(
                        ex,
                        "Failed to publish outbox message Id={OutboxId} Type={EventType}",
                        message.Id,
                        message.Type);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            try { channel?.Dispose(); } catch { /* ignore */ }
            try { connection?.Dispose(); } catch { /* ignore */ }
        }
    }
}
