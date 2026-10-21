using Core.DomainEvents;

namespace Domain.Events;

public sealed record CreditApplicationCancelled(
    Guid CreditApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
