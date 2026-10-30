using Core.DomainEvents;

namespace Domain.Events;

public sealed record CreditApplicationDocumentUploaded(
    Guid CreditApplicationId,
    string ApplicationNumber,
    Guid CustomerId,
    string DocumentType,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
