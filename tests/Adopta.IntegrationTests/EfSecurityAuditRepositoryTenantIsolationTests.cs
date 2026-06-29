using Adopta.Application.Audit;
using Adopta.Infrastructure.Persistence;

namespace Adopta.IntegrationTests;

public sealed class EfSecurityAuditRepositoryTenantIsolationTests
{
    [Fact]
    public async Task Security_audit_add_and_list_preserves_tenant_boundaries()
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
    public async Task Security_audit_cross_tenant_write_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateRepository(databaseName, tenantA);

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(CreateAuditEvent(tenantB, "denied")));
    }

    private static EfSecurityAuditEventRepository CreateRepository(string databaseName, Guid tenantId)
    {
        return new EfSecurityAuditEventRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static SecurityAuditEventRecord CreateAuditEvent(Guid tenantId, string action)
    {
        return new SecurityAuditEventRecord(
            tenantId,
            DateTimeOffset.UtcNow,
            action,
            "success",
            null);
    }
}
