using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Identity;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryTenantMappingRepository : ITenantMappingRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<TenantMappingRecord> _mappings = [];

    public InMemoryTenantMappingRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(TenantMappingRecord mapping, CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, mapping.TenantId);

        lock (_gate)
        {
            _mappings.Add(mapping);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<TenantMappingRecord>> FindAsync(
        string externalTenantId,
        string applicationId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<TenantMappingRecord>>(
                _mappings.Where(mapping =>
                    mapping.TenantId == tenantId
                    && mapping.ExternalTenantId.Equals(externalTenantId, StringComparison.Ordinal)
                    && mapping.ApplicationId.Equals(applicationId, StringComparison.Ordinal))
                .ToArray());
        }
    }
}
