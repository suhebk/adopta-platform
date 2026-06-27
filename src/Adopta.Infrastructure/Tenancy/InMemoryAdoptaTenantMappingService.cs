using Adopta.Application.Abstractions;
using Adopta.Application.Identity;

namespace Adopta.Infrastructure.Tenancy;

public sealed class InMemoryAdoptaTenantMappingService : IAdoptaTenantMappingService
{
    private readonly InMemoryAdoptaTenantMappingStore _store;

    public InMemoryAdoptaTenantMappingService(InMemoryAdoptaTenantMappingStore store)
    {
        _store = store;
    }

    public AdoptaTenantMappingResult MapTenant(string externalTenantId, string applicationId)
    {
        if (string.IsNullOrWhiteSpace(externalTenantId) || string.IsNullOrWhiteSpace(applicationId))
        {
            return AdoptaTenantMappingResult.Unmapped("tenant_mapping_claims_missing");
        }

        var matches = _store.Find(externalTenantId, applicationId);

        return matches.Count switch
        {
            0 => AdoptaTenantMappingResult.Unmapped("tenant_mapping_not_found"),
            1 => AdoptaTenantMappingResult.Mapped(matches.Single().TenantId),
            _ => AdoptaTenantMappingResult.Unmapped("tenant_mapping_ambiguous")
        };
    }
}
