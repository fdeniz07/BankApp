namespace Application.Services.Messaging;

/// <summary>
/// Lab decision-letter email payload (approve/reject). Delivered via RabbitMQ; consumer logs to Seq (no real SMTP).
/// </summary>
public sealed record DecisionLetterRequested(
    string ToEmail,
    string? CustomerName,
    string ApplicationNumber,
    string Decision,
    string Subject,
    string Body,
    DateTimeOffset OccurredOnUtc);
