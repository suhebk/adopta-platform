using Adopta.Application.Audit;
using Adopta.Application.Identity;
using Adopta.Domain.Audit;
using Adopta.Domain.Identity;
using Adopta.Domain.Tenancy;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;

namespace Adopta.UnitTests;

public sealed class TenantScopedRepositoryTests
{
    [Fact]
    public async Task Tenant_can_access_own_application_records()
    {
        var tenantId = Guid.NewGuid();
        var context = BuildTenantContext(tenantId);
        var repository = new InMemoryTenantApplicationRepository(context);
        var application = new TenantApplication(
            Guid.NewGuid(),
            tenantId,
            "Tenant A app",
            new Uri("https://tenant-a.example"));

        await repository.AddAsync(application);

        var stored = await repository.GetByIdAsync(application.Id);
        var list = await repository.ListAsync();

        Assert.Same(application, stored);
        Assert.Single(list);
    }

    [Fact]
    public async Task Tenant_cannot_write_other_tenant_application_record()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repository = new InMemoryTenantApplicationRepository(BuildTenantContext(tenantA));
        var application = new TenantApplication(
            Guid.NewGuid(),
            tenantB,
            "Tenant B app",
            new Uri("https://tenant-b.example"));

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() => repository.AddAsync(application));

        Assert.Equal("Tenant access denied.", ex.Message);
        Assert.DoesNotContain(tenantB.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Missing_tenant_context_is_denied()
    {
        var repository = new InMemoryTenantApplicationRepository(new AdoptionTenantContext());

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() => repository.ListAsync());

        Assert.Equal("Tenant access denied.", ex.Message);
    }

    [Fact]
    public async Task Tenant_mapping_repository_preserves_tenant_boundary()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repository = new InMemoryTenantMappingRepository(BuildTenantContext(tenantA));

        await repository.AddAsync(new TenantMappingRecord(tenantA, "external-a", "app"));
        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.AddAsync(new TenantMappingRecord(tenantB, "external-b", "app")));

        var tenantARecords = await repository.FindAsync("external-a", "app");
        var tenantBRecords = await repository.FindAsync("external-b", "app");

        Assert.Single(tenantARecords);
        Assert.Empty(tenantBRecords);
    }

    [Fact]
    public async Task Authenticated_user_mapping_repository_preserves_tenant_boundary()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repository = new InMemoryAuthenticatedUserMappingRepository(BuildTenantContext(tenantA));
        var userA = BuildUser(tenantA, "subject-a");
        var userB = BuildUser(tenantB, "subject-b");

        await repository.AddAsync(new AuthenticatedUserMappingRecord(tenantA, "subject-a", userA));
        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.AddAsync(new AuthenticatedUserMappingRecord(tenantB, "subject-b", userB)));

        Assert.NotNull(await repository.FindAsync("subject-a"));
        Assert.Null(await repository.FindAsync("subject-b"));
    }

    [Fact]
    public async Task Audit_repositories_preserve_tenant_boundary()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var auditRepository = new InMemoryAuditEventRepository(BuildTenantContext(tenantA));
        var securityAuditRepository = new InMemorySecurityAuditEventRepository(BuildTenantContext(tenantA));

        await auditRepository.AddAsync(new AuditEvent(
            Guid.NewGuid(),
            tenantA,
            Guid.NewGuid(),
            "TenantUpdated",
            "Tenant",
            tenantA.ToString(),
            DateTimeOffset.UtcNow));
        await securityAuditRepository.AddAsync(new SecurityAuditEventRecord(
            tenantA,
            DateTimeOffset.UtcNow,
            "PermissionCheck",
            "Denied",
            "permission_denied"));

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() => auditRepository.AddAsync(new AuditEvent(
            Guid.NewGuid(),
            tenantB,
            Guid.NewGuid(),
            "TenantUpdated",
            "Tenant",
            tenantB.ToString(),
            DateTimeOffset.UtcNow)));
        await Assert.ThrowsAsync<TenantAccessDeniedException>(() => securityAuditRepository.AddAsync(
            new SecurityAuditEventRecord(tenantB, DateTimeOffset.UtcNow, "PermissionCheck", "Denied", null)));

        Assert.Single(await auditRepository.ListAsync());
        Assert.Single(await securityAuditRepository.ListAsync());
    }

    private static AdoptionTenantContext BuildTenantContext(Guid tenantId)
    {
        var context = new AdoptionTenantContext();
        context.SetTenant(tenantId);
        return context;
    }

    private static AdoptionUser BuildUser(Guid tenantId, string subjectId)
    {
        return new AdoptionUser(Guid.NewGuid(), tenantId, subjectId, "Test User");
    }
}
