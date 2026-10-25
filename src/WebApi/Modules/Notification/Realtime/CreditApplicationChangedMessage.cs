namespace WebApi.Modules.Notification.Realtime;

public sealed record CreditApplicationChangedMessage(
    Guid ApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    string EventType,
    DateTimeOffset OccurredOnUtc,
    /// <summary>Requested or approved amount when present on the domain event; null for reject/cancel.</summary>
    decimal? Amount = null,
    /// <summary>ISO currency from credit type (TRY/USD/EUR); clients should display symbols.</summary>
    string? CurrencyCode = null);
