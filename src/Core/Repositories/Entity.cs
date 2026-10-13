namespace Core.Repositories;

public abstract class Entity
{
    public Guid Id { get; set; } = default!;
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }

    public Entity()
    {
        Id = Guid.CreateVersion7();
        CreatedDate = DateTimeOffset.UtcNow;
    }
}
