using Adopta.Application.Authoring;
using Adopta.Application.Runtime;

namespace Adopta.Infrastructure.Persistence;

public sealed class AuthoredContentPublishingHistoryRecord
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ContentId { get; set; }

    public Guid VersionId { get; set; }

    public Guid ActorUserId { get; set; }

    public string Environment { get; set; } = string.Empty;

    public DeliveryChannel Channel { get; set; }

    public string Result { get; set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; set; }

    public static AuthoredContentPublishingHistoryRecord FromAuditRecord(
        AuthoredContentPublishingAuditRecord auditRecord)
    {
        return new AuthoredContentPublishingHistoryRecord
        {
            Id = Guid.NewGuid(),
            TenantId = auditRecord.TenantId,
            ContentId = auditRecord.ContentId,
            VersionId = auditRecord.VersionId,
            ActorUserId = auditRecord.ActorUserId,
            Environment = auditRecord.Environment,
            Channel = auditRecord.Channel,
            Result = auditRecord.Result,
            OccurredAtUtc = auditRecord.OccurredAtUtc
        };
    }

    public AuthoredContentPublishingAuditRecord ToAuditRecord()
    {
        return new AuthoredContentPublishingAuditRecord(
            TenantId,
            ContentId,
            VersionId,
            ActorUserId,
            Environment,
            Channel,
            Result,
            OccurredAtUtc);
    }
}
