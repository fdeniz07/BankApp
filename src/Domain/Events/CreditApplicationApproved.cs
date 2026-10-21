using Core.DomainEvents;

namespace Domain.Events;

public sealed record CreditApplicationApproved(
    Guid CreditApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    decimal ApprovedAmount,
    int ApprovedTermMonths,
    decimal? ApprovedInterestRate,
    string? DecisionBy,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
