using Adopta.Application.Identity;
using Adopta.Domain.Identity;
using Adopta.Infrastructure.Persistence;

namespace Adopta.IntegrationTests;

public sealed class EfMappingRepositoryTenantIsolationTests
{
    [Fact]
    public async Task Tenant_mappings_are_tenant_filtered()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateTenantMappingRepository(databaseName, tenantA);
        var repositoryB = CreateTenantMappingRepository(databaseName, tenantB);

        await repositoryA.AddAsync(new TenantMappingRecord(tenantA, "external-tenant", "app"));
        await repositoryB.AddAsync(new TenantMappingRecord(tenantB, "external-tenant", "app"));

        var mappings = await repositoryA.FindAsync("external-tenant", "app");
        var mapping = Assert.Single(mappings);

        Assert.Equal(tenantA, mapping.TenantId);
    }

    [Fact]
    public async Task Tenant_mapping_cross_tenant_write_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateTenantMappingRepository(databaseName, tenantA);

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(new TenantMappingRecord(tenantB, "external-tenant", "app")));
    }

    [Fact]
    public async Task Authenticated_user_mappings_are_tenant_filtered()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userA = CreateUser(tenantA, "subject-a");
        var userB = CreateUser(tenantB, "subject-b");
        await SeedUsersAsync(databaseName, userA, userB);
        var repositoryA = CreateAuthenticatedUserMappingRepository(databaseName, tenantA);
        var repositoryB = CreateAuthenticatedUserMappingRepository(databaseName, tenantB);

        await repositoryA.AddAsync(new AuthenticatedUserMappingRecord(tenantA, "external-subject", userA));
        await repositoryB.AddAsync(new AuthenticatedUserMappingRecord(tenantB, "external-subject", userB));

        var mapping = await repositoryA.FindAsync("external-subject");

        Assert.NotNull(mapping);
        Assert.Equal(tenantA, mapping.TenantId);
        Assert.Equal(userA.Id, mapping.User.Id);
    }

    [Fact]
    public async Task Authenticated_user_mapping_cross_tenant_write_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userB = CreateUser(tenantB, "subject-b");
        await SeedUsersAsync(databaseName, userB);
        var repositoryA = CreateAuthenticatedUserMappingRepository(databaseName, tenantA);

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(new AuthenticatedUserMappingRecord(tenantB, "external-subject", userB)));
    }

    private static EfTenantMappingRepository CreateTenantMappingRepository(string databaseName, Guid tenantId)
    {
        return new EfTenantMappingRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static EfAuthenticatedUserMappingRepository CreateAuthenticatedUserMappingRepository(
        string databaseName,
        Guid tenantId)
    {
        return new EfAuthenticatedUserMappingRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static async Task SeedUsersAsync(string databaseName, params AdoptionUser[] users)
    {
        await using var dbContext = EfRepositoryTestFactory.CreateDbContext(databaseName);
        dbContext.AdoptionUsers.AddRange(users);
        await dbContext.SaveChangesAsync();
    }

    private static AdoptionUser CreateUser(Guid tenantId, string externalUserId)
    {
        return new AdoptionUser(Guid.NewGuid(), tenantId, externalUserId, "Test User");
    }
}
