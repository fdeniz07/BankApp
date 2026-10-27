namespace Application.Services.Messaging;

/// <summary>
/// Lab decision-letter SMS payload (approve/reject). Delivered via RabbitMQ; consumer logs to Seq (no real SMS gateway).
/// </summary>
public sealed record DecisionSmsRequested(
    string ToPhone,
    string? CustomerName,
    string ApplicationNumber,
    string Decision,
    string Body,
    DateTimeOffset OccurredOnUtc);
