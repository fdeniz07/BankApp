using Microsoft.Extensions.Logging;

namespace Application.Services.Logging;

public class SerilogAuditLogger : IAuditLogger
{
    private readonly ILogger<SerilogAuditLogger> _logger;

    public SerilogAuditLogger(ILogger<SerilogAuditLogger> logger)
    {
        _logger = logger;
    }

    public void Log(
        string category,
        string action,
        string message,
        Guid? actorUserId = null,
        string? actorEmail = null,
        string? targetType = null,
        Guid? targetId = null,
        object? changes = null)
    {
        _logger.LogInformation(
            "BankApp event {EventCategory} {EventAction} {Message} {ActorUserId} {ActorEmail} {TargetType} {TargetId} {@Changes}",
            category,
            action,
            message,
            actorUserId,
            actorEmail,
            targetType,
            targetId,
            changes);
    }
}
