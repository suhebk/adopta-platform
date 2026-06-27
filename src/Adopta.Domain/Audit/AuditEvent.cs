using Adopta.Domain.Common;

namespace Adopta.Domain.Audit;

public sealed class AuditEvent : TenantScopedEntity
{
    public AuditEvent(
        Guid id,
        Guid tenantId,
        Guid actorUserId,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Audit event id is required.", nameof(id));
        }

        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Actor user id is required.", nameof(actorUserId));
        }

        Id = id;
        ActorUserId = actorUserId;
        Action = RequireText(action, nameof(action));
        TargetType = RequireText(targetType, nameof(targetType));
        TargetId = RequireText(targetId, nameof(targetId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid ActorUserId { get; }

    public string Action { get; }

    public string TargetType { get; }

    public string TargetId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value.Trim();
    }
}
