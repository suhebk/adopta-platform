using Adopta.Domain.Tenancy;
using Adopta.Infrastructure.Tenancy;

namespace Adopta.IntegrationTests;

public sealed class TenantIsolationConventionTests
{
    [Fact]
    public void Tenant_scoped_store_returns_only_records_for_requested_tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var store = new InMemoryTenantScopedStore<TenantApplication>();

        store.Add(new TenantApplication(Guid.NewGuid(), tenantA, "Contoso App", new Uri("https://app.contoso.com")));
        store.Add(new TenantApplication(Guid.NewGuid(), tenantB, "Fabrikam App", new Uri("https://app.fabrikam.com")));

        var tenantAApplications = store.ListForTenant(tenantA);

        var application = Assert.Single(tenantAApplications);
        Assert.Equal(tenantA, application.TenantId);
        Assert.Equal("Contoso App", application.Name);
    }
}
