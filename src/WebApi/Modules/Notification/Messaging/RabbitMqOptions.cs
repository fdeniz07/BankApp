namespace WebApi.Modules.Notification.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public bool Enabled { get; set; }
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string EmailConfirmationQueue { get; set; } = "email-confirmation";
    public string DomainEventsQueue { get; set; } = "domain-events";
    public string DecisionLetterQueue { get; set; } = "decision-letter";
}

public sealed class AppPublicOptions
{
    public const string SectionName = "App";

    public string PublicClientBaseUrl { get; set; } = "http://localhost:3000";
}
