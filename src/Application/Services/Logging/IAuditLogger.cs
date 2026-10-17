namespace Application.Services.Logging;

public interface IAuditLogger
{
    void Log(
        string category,
        string action,
        string message,
        Guid? actorUserId = null,
        string? actorEmail = null,
        string? targetType = null,
        Guid? targetId = null,
        object? changes = null);
}
