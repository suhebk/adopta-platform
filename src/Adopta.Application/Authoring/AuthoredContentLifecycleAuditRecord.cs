using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public sealed record AuthoredContentLifecycleAuditRecord(
    Guid TenantId,
    Guid ContentId,
    Guid? VersionId,
    Guid ActorUserId,
    string LifecycleAction,
    ContentLifecycleState FromState,
    ContentLifecycleState ToState,
    string Result,
    DateTimeOffset OccurredAtUtc);
