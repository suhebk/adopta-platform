using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Authoring;
using Adopta.Application.Runtime;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;

namespace Adopta.UnitTests;

public sealed class AuthoringPublishingWorkflowTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Only_approved_content_version_can_be_published()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.Approved);

        var result = await fixture.Workflow.PublishAsync(fixture.Repository, BuildCommand(fixture));

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthoredContentPublishStatus.Succeeded, result.Status);
        Assert.NotNull(result.Bundle);
        Assert.NotNull(result.AuditRecord);
    }

    [Theory]
    [InlineData(ContentLifecycleState.Draft, "publish_denied_draft")]
    [InlineData(ContentLifecycleState.InReview, "publish_denied_in_review")]
    [InlineData(ContentLifecycleState.Published, "publish_denied_already_published")]
    [InlineData(ContentLifecycleState.Archived, "publish_denied_archived")]
    public async Task Non_approved_versions_cannot_be_published(
        ContentLifecycleState state,
        string expectedCode)
    {
        var fixture = await BuildFixtureAsync(state);

        var result = await fixture.Workflow.PublishAsync(fixture.Repository, BuildCommand(fixture));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoredContentPublishStatus.ValidationFailed, result.Status);
        Assert.Null(result.Bundle);
        Assert.Null(result.AuditRecord);
        Assert.Contains(result.Issues, issue => issue.Code == expectedCode);
    }

    [Fact]
    public async Task Publish_command_returns_typed_safe_failures()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.Approved);

        var result = await fixture.Workflow.PublishAsync(
            fixture.Repository,
            new AuthoredContentPublishCommand(
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                "",
                (DeliveryChannel)99,
                default));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthoredContentPublishStatus.InvalidRequest, result.Status);
        Assert.All(result.Issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Code));
            Assert.False(string.IsNullOrWhiteSpace(issue.Path));
            Assert.False(string.IsNullOrWhiteSpace(issue.Message));
            Assert.DoesNotContain(fixture.TenantId.ToString(), issue.Message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Publishing_audit_shape_is_structural_and_safe()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.Approved);
        var actorUserId = Guid.NewGuid();

        var result = await fixture.Workflow.PublishAsync(fixture.Repository, BuildCommand(fixture, actorUserId));

        var audit = Assert.IsType<AuthoredContentPublishingAuditRecord>(result.AuditRecord);
        Assert.Equal(fixture.TenantId, audit.TenantId);
        Assert.Equal(fixture.Content.Id, audit.ContentId);
        Assert.Equal(fixture.Version.Id, audit.VersionId);
        Assert.Equal(actorUserId, audit.ActorUserId);
        Assert.Equal("production", audit.Environment);
        Assert.Equal(DeliveryChannel.Published, audit.Channel);
        Assert.Equal("Succeeded", audit.Result);
        Assert.Equal(Now, audit.OccurredAtUtc);
    }

    [Fact]
    public async Task Publishing_audit_record_can_be_persisted_through_repository_boundary()
    {
        var fixture = await BuildFixtureAsync(ContentLifecycleState.Approved);
        var result = await fixture.Workflow.PublishAsync(fixture.Repository, BuildCommand(fixture));
        var context = new AdoptionTenantContext();
        context.SetTenant(fixture.TenantId);
        var repository = new InMemoryAuthoredContentPublishingHistoryRepository(context);

        await repository.AddAsync(result.AuditRecord!);
        var records = await repository.ListAsync();

        var record = Assert.Single(records);
        Assert.Equal(fixture.TenantId, record.TenantId);
        Assert.Equal(fixture.Content.Id, record.ContentId);
        Assert.Equal(fixture.Version.Id, record.VersionId);
        Assert.Equal("Succeeded", record.Result);
    }

    private static AuthoredContentPublishCommand BuildCommand(
        PublishingFixture fixture,
        Guid? actorUserId = null)
    {
        return new AuthoredContentPublishCommand(
            fixture.TenantId,
            fixture.Content.Id,
            fixture.Version.Id,
            actorUserId ?? Guid.NewGuid(),
            "production",
            DeliveryChannel.Published,
            Now);
    }

    private static async Task<PublishingFixture> BuildFixtureAsync(
        ContentLifecycleState state,
        string contentKey = "billing.submit")
    {
        var tenantId = Guid.NewGuid();
        var context = new AdoptionTenantContext();
        context.SetTenant(tenantId);
        IAuthoredContentRepository repository = new InMemoryAuthoredContentRepository(context);
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
            contentKey,
            "Submit return",
            [version]);

        await repository.AddAsync(content);

        return new PublishingFixture(
            tenantId,
            content,
            version,
            repository,
            new AuthoredContentPublishingWorkflow());
    }

    private sealed record PublishingFixture(
        Guid TenantId,
        AuthoredContentItem Content,
        AuthoredContentVersion Version,
        IAuthoredContentRepository Repository,
        AuthoredContentPublishingWorkflow Workflow);
}
