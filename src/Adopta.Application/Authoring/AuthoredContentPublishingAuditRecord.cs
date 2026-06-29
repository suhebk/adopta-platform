using Adopta.Application.Runtime;

namespace Adopta.Application.Authoring;

public sealed record AuthoredContentPublishingAuditRecord(
    Guid TenantId,
    Guid ContentId,
    Guid VersionId,
    Guid ActorUserId,
    string Environment,
    DeliveryChannel Channel,
    string Result,
    DateTimeOffset OccurredAtUtc);
