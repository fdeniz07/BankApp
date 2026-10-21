using Core.DomainEvents;

namespace Domain.Events;

public sealed record CreditApplicationRejected(
    Guid CreditApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    string RejectionReason,
    string? DecisionBy,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
