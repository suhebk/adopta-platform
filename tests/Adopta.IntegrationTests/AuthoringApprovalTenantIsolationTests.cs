using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Authoring;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class AuthoringApprovalTenantIsolationTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 29, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Repository_involved_workflow_remains_tenant_scoped()
    {
        var tenantId = Guid.NewGuid();
        var repository = BuildRepository(tenantId);
        var workflow = new AuthoredContentApprovalWorkflow();
        var content = BuildContent(tenantId, ContentLifecycleState.Draft);
        var version = Assert.Single(content.Versions);

        await repository.AddAsync(content);

        var result = await workflow.RequestReviewAsync(
            repository,
            new AuthoredContentReviewRequest(
                tenantId,
                content.Id,
                version.Id,
                Guid.NewGuid(),
                Now));

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.AuditRecord?.TenantId);
    }

    [Fact]
    public async Task Cross_tenant_review_access_is_denied_or_hidden_safely()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tenantARepository = BuildRepository(tenantA);
        var tenantBRepository = BuildRepository(tenantB);
        var workflow = new AuthoredContentApprovalWorkflow();
        var tenantBContent = BuildContent(tenantB, ContentLifecycleState.Draft);
        var tenantBVersion = Assert.Single(tenantBContent.Versions);

        await tenantBRepository.AddAsync(tenantBContent);

        var hiddenResult = await workflow.RequestReviewAsync(
            tenantARepository,
            new AuthoredContentReviewRequest(
                tenantA,
                tenantBContent.Id,
                tenantBVersion.Id,
                Guid.NewGuid(),
                Now));

        Assert.Equal(AuthoredContentLifecycleDecisionStatus.NotFound, hiddenResult.Status);
        Assert.Null(hiddenResult.AuditRecord);

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            workflow.RequestReviewAsync(
                tenantARepository,
                new AuthoredContentReviewRequest(
                    tenantB,
                    tenantBContent.Id,
                    tenantBVersion.Id,
                    Guid.NewGuid(),
                    Now)));

        Assert.Equal("Tenant access denied.", ex.Message);
        Assert.DoesNotContain(tenantB.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Cross_tenant_approval_access_is_denied_or_hidden_safely()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tenantARepository = BuildRepository(tenantA);
        var tenantBRepository = BuildRepository(tenantB);
        var workflow = new AuthoredContentApprovalWorkflow();
        var tenantBContent = BuildContent(tenantB, ContentLifecycleState.InReview);
        var tenantBVersion = Assert.Single(tenantBContent.Versions);

        await tenantBRepository.AddAsync(tenantBContent);

        var hiddenResult = await workflow.DecideAsync(
            tenantARepository,
            new AuthoredContentApprovalDecision(
                tenantA,
                tenantBContent.Id,
                tenantBVersion.Id,
                Guid.NewGuid(),
                AuthoredContentApprovalDecisionKind.Approve,
                Now));

        Assert.Equal(AuthoredContentLifecycleDecisionStatus.NotFound, hiddenResult.Status);
        Assert.Null(hiddenResult.AuditRecord);

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            workflow.DecideAsync(
                tenantARepository,
                new AuthoredContentApprovalDecision(
                    tenantB,
                    tenantBContent.Id,
                    tenantBVersion.Id,
                    Guid.NewGuid(),
                    AuthoredContentApprovalDecisionKind.Approve,
                    Now)));

        Assert.Equal("Tenant access denied.", ex.Message);
        Assert.DoesNotContain(tenantB.ToString(), ex.Message, StringComparison.Ordinal);
    }

    private static IAuthoredContentRepository BuildRepository(Guid tenantId)
    {
        var services = new ServiceCollection();
        var context = new AdoptionTenantContext();
        context.SetTenant(tenantId);

        services.AddScoped<AdoptionTenantContext>(_ => context);
        services.AddScoped<IAdoptionTenantContext>(_ => context);
        services.AddScoped<IAuthoredContentRepository, InMemoryAuthoredContentRepository>();

        return services.BuildServiceProvider().GetRequiredService<IAuthoredContentRepository>();
    }

    private static AuthoredContentItem BuildContent(Guid tenantId, ContentLifecycleState state)
    {
        return new AuthoredContentItem(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            AuthoredContentType.Tooltip,
            "billing.submit",
            "Submit return",
            [
                new AuthoredContentVersion(
                    Guid.NewGuid(),
                    "1.0.0",
                    state,
                    Now)
            ]);
    }
}
