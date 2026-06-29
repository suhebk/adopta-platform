using Adopta.Application.Authoring;
using Adopta.Domain.Authoring;

namespace Adopta.Infrastructure.Persistence;

public sealed class AuthoredContentLifecycleHistoryRecord
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ContentId { get; set; }

    public Guid? VersionId { get; set; }

    public Guid ActorUserId { get; set; }

    public string LifecycleAction { get; set; } = string.Empty;

    public ContentLifecycleState FromState { get; set; }

    public ContentLifecycleState ToState { get; set; }

    public string Result { get; set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; set; }

    public static AuthoredContentLifecycleHistoryRecord FromAuditRecord(
        AuthoredContentLifecycleAuditRecord auditRecord)
    {
        return new AuthoredContentLifecycleHistoryRecord
        {
            Id = Guid.NewGuid(),
            TenantId = auditRecord.TenantId,
            ContentId = auditRecord.ContentId,
            VersionId = auditRecord.VersionId,
            ActorUserId = auditRecord.ActorUserId,
            LifecycleAction = auditRecord.LifecycleAction,
            FromState = auditRecord.FromState,
            ToState = auditRecord.ToState,
            Result = auditRecord.Result,
            OccurredAtUtc = auditRecord.OccurredAtUtc
        };
    }

    public AuthoredContentLifecycleAuditRecord ToAuditRecord()
    {
        return new AuthoredContentLifecycleAuditRecord(
            TenantId,
            ContentId,
            VersionId,
            ActorUserId,
            LifecycleAction,
            FromState,
            ToState,
            Result,
            OccurredAtUtc);
    }
}
