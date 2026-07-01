using Adopta.Domain.Authoring;
using Adopta.Application.Runtime;

namespace Adopta.Api.Authoring;

public sealed record CreateAuthoredContentRequest(
    Guid ApplicationId,
    string ContentKey,
    string Title,
    string Version);

public sealed record AuthoredContentResponse(
    Guid Id,
    Guid TenantId,
    Guid ApplicationId,
    string ContentKey,
    string Title,
    IReadOnlyCollection<AuthoredContentVersionResponse> Versions,
    AuthoredContentReadSummaryResponse? Summary);

public sealed record AuthoredContentVersionResponse(
    Guid Id,
    string Version,
    ContentLifecycleState LifecycleState,
    DateTimeOffset CreatedAtUtc);

public sealed record AuthoredContentListResponse(
    IReadOnlyCollection<AuthoredContentResponse> Items);

public sealed record AuthoredContentReadSummaryResponse(
    int LifecycleEventCount,
    int PublishingEventCount,
    string LatestSafeActivity,
    DateTimeOffset? LatestActivityAtUtc,
    AuthoredContentLatestPublishSummaryResponse? LatestPublish);

public sealed record AuthoredContentLatestPublishSummaryResponse(
    string Status,
    string Environment,
    DeliveryChannel Channel,
    DateTimeOffset OccurredAtUtc);

public sealed record RequestReviewRequest(
    DateTimeOffset? RequestedAtUtc);

public sealed record ApprovalDecisionRequest(
    DateTimeOffset? DecidedAtUtc);

public sealed record PublishAuthoredContentRequest(
    string Environment,
    DeliveryChannel Channel,
    DateTimeOffset? RequestedAtUtc);

public sealed record AuthoringCommandResponse(
    bool Succeeded,
    string Status,
    AuthoredContentResponse? Content,
    LifecycleDecisionAuditResponse? Audit,
    IReadOnlyCollection<AuthoringIssueResponse> Issues);

public sealed record LifecycleDecisionAuditResponse(
    Guid TenantId,
    Guid ContentId,
    Guid? VersionId,
    Guid ActorUserId,
    string LifecycleAction,
    ContentLifecycleState FromState,
    ContentLifecycleState ToState,
    string Result,
    DateTimeOffset OccurredAtUtc);

public sealed record PublishAuthoredContentResponse(
    bool Succeeded,
    string Status,
    PublishBundleMetadataResponse? Bundle,
    PublishingAuditResponse? Audit,
    IReadOnlyCollection<AuthoringIssueResponse> Issues);

public sealed record PublishBundleMetadataResponse(
    string BundleId,
    Guid TenantId,
    Guid ApplicationId,
    string Environment,
    DeliveryChannel Channel,
    string Version,
    DateTimeOffset GeneratedAtUtc,
    int ItemCount);

public sealed record PublishingAuditResponse(
    Guid TenantId,
    Guid ContentId,
    Guid VersionId,
    Guid ActorUserId,
    string Environment,
    DeliveryChannel Channel,
    string Result,
    DateTimeOffset OccurredAtUtc);

public sealed record AuthoringIssueResponse(
    string Code,
    string Path,
    string Message);
