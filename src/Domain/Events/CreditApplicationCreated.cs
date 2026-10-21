using Core.DomainEvents;

namespace Domain.Events;

public sealed record CreditApplicationCreated(
    Guid CreditApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    Guid CreditSubTypeId,
    decimal RequestedAmount,
    int RequestedTermMonths,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
