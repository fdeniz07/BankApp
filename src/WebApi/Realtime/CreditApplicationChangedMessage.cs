namespace WebApi.Realtime;

public sealed record CreditApplicationChangedMessage(
    Guid ApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    string EventType,
    DateTimeOffset OccurredOnUtc);
