namespace WebApi.Modules.Notification.Realtime;

public sealed record CreditApplicationChangedMessage(
    Guid ApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    string EventType,
    DateTimeOffset OccurredOnUtc,
    /// <summary>Requested or approved amount when present on the domain event; null for reject/cancel.</summary>
    decimal? Amount = null);
