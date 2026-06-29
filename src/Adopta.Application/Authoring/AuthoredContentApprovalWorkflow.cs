using Adopta.Application.Abstractions.Authoring;
using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public sealed class AuthoredContentApprovalWorkflow
{
    public async Task<AuthoredContentLifecycleDecisionResult> RequestReviewAsync(
        IAuthoredContentRepository repository,
        AuthoredContentReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var issues = ValidateReviewRequest(request);
        if (issues.Count > 0)
        {
            return AuthoredContentLifecycleDecisionResult.InvalidRequest(issues);
        }

        var version = await ResolveVersionAsync(
            repository,
            request.TenantId,
            request.ContentId,
            request.VersionId,
            cancellationToken);

        if (version is null)
        {
            return AuthoredContentLifecycleDecisionResult.NotFound(
                [Issue("authored_content_not_found", "content", "Authored content was not found.")]);
        }

        return TryCreateResult(
            request.TenantId,
            request.ContentId,
            request.VersionId,
            request.ActorUserId,
            "RequestReview",
            version.LifecycleState,
            ContentLifecycleState.InReview,
            request.RequestedAtUtc);
    }

    public async Task<AuthoredContentLifecycleDecisionResult> DecideAsync(
        IAuthoredContentRepository repository,
        AuthoredContentApprovalDecision decision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var issues = ValidateApprovalDecision(decision);
        if (issues.Count > 0)
        {
            return AuthoredContentLifecycleDecisionResult.InvalidRequest(issues);
        }

        var version = await ResolveVersionAsync(
            repository,
            decision.TenantId,
            decision.ContentId,
            decision.VersionId,
            cancellationToken);

        if (version is null)
        {
            return AuthoredContentLifecycleDecisionResult.NotFound(
                [Issue("authored_content_not_found", "content", "Authored content was not found.")]);
        }

        var toState = decision.Decision switch
        {
            AuthoredContentApprovalDecisionKind.Approve => ContentLifecycleState.Approved,
            AuthoredContentApprovalDecisionKind.Reject => ContentLifecycleState.Draft,
            _ => (ContentLifecycleState?)null
        };

        if (toState is null)
        {
            return AuthoredContentLifecycleDecisionResult.InvalidRequest(
                [Issue("invalid_approval_decision", "decision.decision", "Approval decision is invalid.")],
                version.LifecycleState);
        }

        return TryCreateResult(
            decision.TenantId,
            decision.ContentId,
            decision.VersionId,
            decision.ActorUserId,
            decision.Decision == AuthoredContentApprovalDecisionKind.Approve ? "Approve" : "Reject",
            version.LifecycleState,
            toState.Value,
            decision.DecidedAtUtc);
    }

    private static async Task<AuthoredContentVersion?> ResolveVersionAsync(
        IAuthoredContentRepository repository,
        Guid tenantId,
        Guid contentId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var content = await repository.GetByIdAsync(tenantId, contentId, cancellationToken);

        return content?.Versions.SingleOrDefault(version => version.Id == versionId);
    }

    private static AuthoredContentLifecycleDecisionResult TryCreateResult(
        Guid tenantId,
        Guid contentId,
        Guid versionId,
        Guid actorUserId,
        string action,
        ContentLifecycleState fromState,
        ContentLifecycleState toState,
        DateTimeOffset occurredAtUtc)
    {
        if (!ContentLifecycleTransition.IsAllowed(fromState, toState))
        {
            return AuthoredContentLifecycleDecisionResult.InvalidRequest(
                [Issue("invalid_lifecycle_decision", "decision", "Lifecycle decision is not allowed.")],
                fromState);
        }

        var auditRecord = new AuthoredContentLifecycleAuditRecord(
            tenantId,
            contentId,
            versionId,
            actorUserId,
            action,
            fromState,
            toState,
            "Succeeded",
            occurredAtUtc);

        return AuthoredContentLifecycleDecisionResult.Succeeded(fromState, toState, auditRecord);
    }

    private static IReadOnlyCollection<AuthoredContentValidationIssue> ValidateReviewRequest(
        AuthoredContentReviewRequest request)
    {
        var issues = new List<AuthoredContentValidationIssue>();

        RequireGuid(request.TenantId, "request.tenantId", issues);
        RequireGuid(request.ContentId, "request.contentId", issues);
        RequireGuid(request.VersionId, "request.versionId", issues);
        RequireGuid(request.ActorUserId, "request.actorUserId", issues);
        RequireTimestamp(request.RequestedAtUtc, "request.requestedAtUtc", issues);

        return issues;
    }

    private static IReadOnlyCollection<AuthoredContentValidationIssue> ValidateApprovalDecision(
        AuthoredContentApprovalDecision decision)
    {
        var issues = new List<AuthoredContentValidationIssue>();

        RequireGuid(decision.TenantId, "decision.tenantId", issues);
        RequireGuid(decision.ContentId, "decision.contentId", issues);
        RequireGuid(decision.VersionId, "decision.versionId", issues);
        RequireGuid(decision.ActorUserId, "decision.actorUserId", issues);
        RequireTimestamp(decision.DecidedAtUtc, "decision.decidedAtUtc", issues);

        if (!Enum.IsDefined(decision.Decision))
        {
            issues.Add(Issue("invalid_approval_decision", "decision.decision", "Approval decision is invalid."));
        }

        return issues;
    }

    private static void RequireGuid(
        Guid value,
        string path,
        ICollection<AuthoredContentValidationIssue> issues)
    {
        if (value == Guid.Empty)
        {
            issues.Add(Issue("invalid_lifecycle_decision", path, "Value is required."));
        }
    }

    private static void RequireTimestamp(
        DateTimeOffset value,
        string path,
        ICollection<AuthoredContentValidationIssue> issues)
    {
        if (value == default)
        {
            issues.Add(Issue("invalid_lifecycle_decision", path, "Timestamp is required."));
        }
    }

    private static AuthoredContentValidationIssue Issue(
        string code,
        string path,
        string message)
    {
        return new AuthoredContentValidationIssue(code, path, message);
    }
}
