namespace Core.DomainEvents;

public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
