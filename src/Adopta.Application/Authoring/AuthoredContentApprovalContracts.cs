using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public enum AuthoredContentApprovalDecisionKind
{
    Approve = 0,
    Reject = 1
}

public enum AuthoredContentLifecycleDecisionStatus
{
    Succeeded = 0,
    InvalidRequest = 1,
    NotFound = 2
}

public sealed record AuthoredContentReviewRequest(
    Guid TenantId,
    Guid ContentId,
    Guid VersionId,
    Guid ActorUserId,
    DateTimeOffset RequestedAtUtc);

public sealed record AuthoredContentApprovalDecision(
    Guid TenantId,
    Guid ContentId,
    Guid VersionId,
    Guid ActorUserId,
    AuthoredContentApprovalDecisionKind Decision,
    DateTimeOffset DecidedAtUtc);

public sealed record AuthoredContentLifecycleDecisionResult(
    AuthoredContentLifecycleDecisionStatus Status,
    ContentLifecycleState? FromState,
    ContentLifecycleState? ToState,
    IReadOnlyCollection<AuthoredContentValidationIssue> Issues,
    AuthoredContentLifecycleAuditRecord? AuditRecord)
{
    public bool IsSuccess => Status == AuthoredContentLifecycleDecisionStatus.Succeeded;

    public static AuthoredContentLifecycleDecisionResult Succeeded(
        ContentLifecycleState fromState,
        ContentLifecycleState toState,
        AuthoredContentLifecycleAuditRecord auditRecord)
    {
        return new AuthoredContentLifecycleDecisionResult(
            AuthoredContentLifecycleDecisionStatus.Succeeded,
            fromState,
            toState,
            [],
            auditRecord);
    }

    public static AuthoredContentLifecycleDecisionResult InvalidRequest(
        IReadOnlyCollection<AuthoredContentValidationIssue> issues,
        ContentLifecycleState? fromState = null)
    {
        return new AuthoredContentLifecycleDecisionResult(
            AuthoredContentLifecycleDecisionStatus.InvalidRequest,
            fromState,
            null,
            issues,
            null);
    }

    public static AuthoredContentLifecycleDecisionResult NotFound(
        IReadOnlyCollection<AuthoredContentValidationIssue> issues)
    {
        return new AuthoredContentLifecycleDecisionResult(
            AuthoredContentLifecycleDecisionStatus.NotFound,
            null,
            null,
            issues,
            null);
    }
}
