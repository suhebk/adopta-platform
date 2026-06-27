using Adopta.Infrastructure.Tenancy;

namespace Adopta.UnitTests;

public sealed class TenantMappingServiceTests
{
    [Fact]
    public void Maps_external_tenant_and_application_to_internal_tenant()
    {
        var internalTenantId = Guid.NewGuid();
        var store = new InMemoryAdoptaTenantMappingStore();
        store.Add("external-tenant", "application", internalTenantId);
        var service = new InMemoryAdoptaTenantMappingService(store);

        var result = service.MapTenant("external-tenant", "application");

        Assert.True(result.IsMapped);
        Assert.Equal(internalTenantId, result.TenantId);
    }

    [Fact]
    public void Fails_closed_when_mapping_is_missing()
    {
        var service = new InMemoryAdoptaTenantMappingService(new InMemoryAdoptaTenantMappingStore());

        var result = service.MapTenant("external-tenant", "application");

        Assert.False(result.IsMapped);
        Assert.Equal("tenant_mapping_not_found", result.FailureCode);
    }

    [Fact]
    public void Fails_closed_when_mapping_is_ambiguous()
    {
        var store = new InMemoryAdoptaTenantMappingStore();
        store.Add("external-tenant", "application", Guid.NewGuid());
        store.Add("external-tenant", "application", Guid.NewGuid());
        var service = new InMemoryAdoptaTenantMappingService(store);

        var result = service.MapTenant("external-tenant", "application");

        Assert.False(result.IsMapped);
        Assert.Equal("tenant_mapping_ambiguous", result.FailureCode);
    }
}
