using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Adopta.IntegrationTests;

internal static class EfRepositoryTestFactory
{
    public static AdoptaDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AdoptaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AdoptaDbContext(options);
    }

    public static AdoptionTenantContext CreateTenantContext(Guid? tenantId = null)
    {
        var tenantContext = new AdoptionTenantContext();
        if (tenantId.HasValue)
        {
            tenantContext.SetTenant(tenantId.Value);
        }

        return tenantContext;
    }
}
