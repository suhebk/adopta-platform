using Adopta.Application.Authoring;
using Adopta.Application.Runtime;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Persistence;

namespace Adopta.IntegrationTests;

public sealed class EfAuthoringHistoryRepositoryTenantIsolationTests
{
    [Fact]
    public async Task Lifecycle_history_add_and_list_preserves_tenant_boundaries()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateLifecycleRepository(databaseName, tenantA);
        var repositoryB = CreateLifecycleRepository(databaseName, tenantB);

        await repositoryA.AddAsync(CreateLifecycleRecord(tenantA, "Approve"));
        await repositoryB.AddAsync(CreateLifecycleRecord(tenantB, "Reject"));

        var records = await repositoryA.ListAsync();
        var record = Assert.Single(records);

        Assert.Equal(tenantA, record.TenantId);
        Assert.Equal("Approve", record.LifecycleAction);
    }

    [Fact]
    public async Task Publishing_history_add_and_list_preserves_tenant_boundaries()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreatePublishingRepository(databaseName, tenantA);
        var repositoryB = CreatePublishingRepository(databaseName, tenantB);

        await repositoryA.AddAsync(CreatePublishingRecord(tenantA, "prod"));
        await repositoryB.AddAsync(CreatePublishingRecord(tenantB, "qa"));

        var records = await repositoryA.ListAsync();
        var record = Assert.Single(records);

        Assert.Equal(tenantA, record.TenantId);
        Assert.Equal("prod", record.Environment);
    }

    [Fact]
    public async Task Lifecycle_history_cross_tenant_write_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateLifecycleRepository(databaseName, tenantA);

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(CreateLifecycleRecord(tenantB, "Approve")));
    }

    [Fact]
    public async Task Publishing_history_cross_tenant_write_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreatePublishingRepository(databaseName, tenantA);

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(CreatePublishingRecord(tenantB, "prod")));
    }

    [Fact]
    public async Task Lifecycle_history_missing_tenant_context_fails_closed()
    {
        var repository = new EfAuthoredContentLifecycleHistoryRepository(
            EfRepositoryTestFactory.CreateDbContext(Guid.NewGuid().ToString("N")),
            EfRepositoryTestFactory.CreateTenantContext());

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.ListAsync());
    }

    [Fact]
    public async Task Publishing_history_missing_tenant_context_fails_closed()
    {
        var repository = new EfAuthoredContentPublishingHistoryRepository(
            EfRepositoryTestFactory.CreateDbContext(Guid.NewGuid().ToString("N")),
            EfRepositoryTestFactory.CreateTenantContext());

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.ListAsync());
    }

    private static EfAuthoredContentLifecycleHistoryRepository CreateLifecycleRepository(
        string databaseName,
        Guid tenantId)
    {
        return new EfAuthoredContentLifecycleHistoryRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static EfAuthoredContentPublishingHistoryRepository CreatePublishingRepository(
        string databaseName,
        Guid tenantId)
    {
        return new EfAuthoredContentPublishingHistoryRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static AuthoredContentLifecycleAuditRecord CreateLifecycleRecord(Guid tenantId, string action)
    {
        return new AuthoredContentLifecycleAuditRecord(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            action,
            ContentLifecycleState.InReview,
            ContentLifecycleState.Approved,
            "Succeeded",
            DateTimeOffset.UtcNow);
    }

    private static AuthoredContentPublishingAuditRecord CreatePublishingRecord(Guid tenantId, string environment)
    {
        return new AuthoredContentPublishingAuditRecord(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            environment,
            DeliveryChannel.Published,
            "Succeeded",
            DateTimeOffset.UtcNow);
    }
}
