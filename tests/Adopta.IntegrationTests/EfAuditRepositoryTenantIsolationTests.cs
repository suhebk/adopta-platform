using Adopta.Domain.Audit;
using Adopta.Infrastructure.Persistence;

namespace Adopta.IntegrationTests;

public sealed class EfAuditRepositoryTenantIsolationTests
{
    [Fact]
    public async Task Audit_add_and_list_preserves_tenant_boundaries()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateRepository(databaseName, tenantA);
        var repositoryB = CreateRepository(databaseName, tenantB);

        await repositoryA.AddAsync(CreateAuditEvent(tenantA, "allowed"));
        await repositoryB.AddAsync(CreateAuditEvent(tenantB, "hidden"));

        var events = await repositoryA.ListAsync();
        var auditEvent = Assert.Single(events);

        Assert.Equal(tenantA, auditEvent.TenantId);
        Assert.Equal("allowed", auditEvent.Action);
    }

    [Fact]
    public async Task Audit_cross_tenant_write_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateRepository(databaseName, tenantA);

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(CreateAuditEvent(tenantB, "denied")));
    }

    [Fact]
    public async Task Audit_missing_tenant_context_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var repository = new EfAuditEventRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext());

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.ListAsync());
    }

    private static EfAuditEventRepository CreateRepository(string databaseName, Guid tenantId)
    {
        return new EfAuditEventRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static AuditEvent CreateAuditEvent(Guid tenantId, string action)
    {
        return new AuditEvent(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            action,
            "content",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow);
    }
}
