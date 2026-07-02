using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Authoring;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Tenancy;

namespace Adopta.UnitTests;

public sealed class AuthoringApprovalWorkflowTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 29, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Requesting_review_from_draft_succeeds()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.Draft);

        var result = await fixture.Workflow.RequestReviewAsync(
            fixture.Repository,
            new AuthoredContentReviewRequest(
                fixture.TenantId,
                fixture.Content.Id,
                fixture.Version.Id,
                Guid.NewGuid(),
                Now));

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentLifecycleState.Draft, result.FromState);
        Assert.Equal(ContentLifecycleState.InReview, result.ToState);
        Assert.NotNull(result.AuditRecord);
        Assert.Equal("RequestReview", result.AuditRecord.LifecycleAction);
    }

    [Theory]
    [InlineData(ContentLifecycleState.InReview)]
    [InlineData(ContentLifecycleState.Approved)]
    [InlineData(ContentLifecycleState.Published)]
    [InlineData(ContentLifecycleState.Archived)]
    public async Task Requesting_review_from_invalid_states_fails_safely(ContentLifecycleState state)
    {
        var fixture = await BuildFixtureAsync(state);

        var result = await fixture.Workflow.RequestReviewAsync(
            fixture.Repository,
            new AuthoredContentReviewRequest(
                fixture.TenantId,
                fixture.Content.Id,
                fixture.Version.Id,
                Guid.NewGuid(),
                Now));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoredContentLifecycleDecisionStatus.InvalidRequest, result.Status);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_lifecycle_decision");
        Assert.Null(result.AuditRecord);
    }

    [Fact]
    public async Task Approving_from_in_review_succeeds()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.InReview);

        var result = await fixture.Workflow.DecideAsync(
            fixture.Repository,
            BuildDecision(fixture, AuthoredContentApprovalDecisionKind.Approve));

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentLifecycleState.InReview, result.FromState);
        Assert.Equal(ContentLifecycleState.Approved, result.ToState);
        Assert.Equal("Approve", result.AuditRecord?.LifecycleAction);
    }

    [Fact]
    public async Task Rejecting_from_in_review_returns_to_draft()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.InReview);

        var result = await fixture.Workflow.DecideAsync(
            fixture.Repository,
            BuildDecision(fixture, AuthoredContentApprovalDecisionKind.Reject));

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentLifecycleState.InReview, result.FromState);
        Assert.Equal(ContentLifecycleState.Draft, result.ToState);
        Assert.Equal("Reject", result.AuditRecord?.LifecycleAction);
    }

    [Theory]
    [InlineData(ContentLifecycleState.Draft)]
    [InlineData(ContentLifecycleState.Published)]
    [InlineData(ContentLifecycleState.Archived)]
    public async Task Approving_from_unsafe_states_fails_safely(ContentLifecycleState state)
    {
        var fixture = await BuildFixtureAsync(state);

        var result = await fixture.Workflow.DecideAsync(
            fixture.Repository,
            BuildDecision(fixture, AuthoredContentApprovalDecisionKind.Approve));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoredContentLifecycleDecisionStatus.InvalidRequest, result.Status);
        Assert.Equal(state, result.FromState);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_lifecycle_decision");
        Assert.Null(result.AuditRecord);
    }

    [Theory]
    [InlineData(AuthoredContentApprovalDecisionKind.Approve)]
    [InlineData(AuthoredContentApprovalDecisionKind.Reject)]
    public async Task Archived_content_cannot_re_enter_workflow(AuthoredContentApprovalDecisionKind decision)
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.Archived);

        var result = await fixture.Workflow.DecideAsync(
            fixture.Repository,
            BuildDecision(fixture, decision));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoredContentLifecycleDecisionStatus.InvalidRequest, result.Status);
    }

    [Fact]
    public async Task Invalid_decisions_return_typed_non_sensitive_issues()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.InReview);

        var result = await fixture.Workflow.DecideAsync(
            fixture.Repository,
            new AuthoredContentApprovalDecision(
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                (AuthoredContentApprovalDecisionKind)99,
                default));

        Assert.False(result.IsSuccess);
        Assert.All(result.Issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Code));
            Assert.False(string.IsNullOrWhiteSpace(issue.Path));
            Assert.False(string.IsNullOrWhiteSpace(issue.Message));
            Assert.DoesNotContain(fixture.TenantId.ToString(), issue.Message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Audit_record_shape_contains_only_safe_structural_fields()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.InReview);
        var actorUserId = Guid.NewGuid();

        var result = await fixture.Workflow.DecideAsync(
            fixture.Repository,
            new AuthoredContentApprovalDecision(
                fixture.TenantId,
                fixture.Content.Id,
                fixture.Version.Id,
                actorUserId,
                AuthoredContentApprovalDecisionKind.Approve,
                Now));

        var audit = Assert.IsType<AuthoredContentLifecycleAuditRecord>(result.AuditRecord);
        Assert.Equal(fixture.TenantId, audit.TenantId);
        Assert.Equal(fixture.Content.Id, audit.ContentId);
        Assert.Equal(fixture.Version.Id, audit.VersionId);
        Assert.Equal(actorUserId, audit.ActorUserId);
        Assert.Equal("Approve", audit.LifecycleAction);
        Assert.Equal(ContentLifecycleState.InReview, audit.FromState);
        Assert.Equal(ContentLifecycleState.Approved, audit.ToState);
        Assert.Equal("Succeeded", audit.Result);
        Assert.Equal(Now, audit.OccurredAtUtc);
    }

    private static AuthoredContentApprovalDecision BuildDecision(
        WorkflowFixture fixture,
        AuthoredContentApprovalDecisionKind decision)
    {
        return new AuthoredContentApprovalDecision(
            fixture.TenantId,
            fixture.Content.Id,
            fixture.Version.Id,
            Guid.NewGuid(),
            decision,
            Now);
    }

    private static async Task<WorkflowFixture> BuildFixtureAsync(ContentLifecycleState state)
    {
        var tenantId = Guid.NewGuid();
        var version = new AuthoredContentVersion(
            Guid.NewGuid(),
            "1.0.0",
            state,
            Now);
        var content = new AuthoredContentItem(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            AuthoredContentType.Tooltip,
            "billing.submit",
            "Submit return",
            [version]);
        var repository = BuildRepository(tenantId);
        await repository.AddAsync(content);

        return new WorkflowFixture(
            tenantId,
            content,
            version,
            repository,
            new AuthoredContentApprovalWorkflow());
    }

    private static IAuthoredContentRepository BuildRepository(Guid tenantId)
    {
        var context = new AdoptionTenantContext();
        context.SetTenant(tenantId);

        return new InMemoryAuthoredContentRepository(context);
    }

    private sealed record WorkflowFixture(
        Guid TenantId,
        AuthoredContentItem Content,
        AuthoredContentVersion Version,
        IAuthoredContentRepository Repository,
        AuthoredContentApprovalWorkflow Workflow);
}
