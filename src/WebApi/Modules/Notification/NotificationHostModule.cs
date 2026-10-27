using Application.Services.Messaging;
using WebApi.Modules.Notification.Messaging;
using WebApi.Modules.Notification.Realtime;

namespace WebApi.Modules.Notification;

/// <summary>
/// Host-side Notification module: RabbitMQ messaging + SignalR realtime.
/// </summary>
public static class NotificationHostModule
{
    public static IServiceCollection AddNotificationHostModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEmailConfirmationPublisher, RabbitMqEmailConfirmationPublisher>();
        services.AddSingleton<IDecisionLetterPublisher, RabbitMqDecisionLetterPublisher>();
        services.AddSingleton<IDecisionSmsPublisher, RabbitMqDecisionSmsPublisher>();
        services.AddHostedService<EmailConfirmationConsumerHostedService>();
        services.AddHostedService<DecisionLetterConsumerHostedService>();
        services.AddHostedService<DecisionSmsConsumerHostedService>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        services.AddHostedService<DomainEventsConsumerHostedService>();
        services.AddSignalR();
        services.AddSingleton<ICreditApplicationRealtimeNotifier, CreditApplicationRealtimeNotifier>();
        return services;
    }
}
