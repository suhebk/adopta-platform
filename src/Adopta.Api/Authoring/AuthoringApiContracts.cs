using Adopta.Domain.Authoring;

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
    IReadOnlyCollection<AuthoredContentVersionResponse> Versions);

public sealed record AuthoredContentVersionResponse(
    Guid Id,
    string Version,
    ContentLifecycleState LifecycleState,
    DateTimeOffset CreatedAtUtc);

public sealed record AuthoredContentListResponse(
    IReadOnlyCollection<AuthoredContentResponse> Items);

public sealed record RequestReviewRequest(
    DateTimeOffset? RequestedAtUtc);

public sealed record ApprovalDecisionRequest(
    DateTimeOffset? DecidedAtUtc);

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

public sealed record AuthoringIssueResponse(
    string Code,
    string Path,
    string Message);
