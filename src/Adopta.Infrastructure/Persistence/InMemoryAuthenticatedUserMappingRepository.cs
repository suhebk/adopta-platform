using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Identity;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryAuthenticatedUserMappingRepository : IAuthenticatedUserMappingRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<AuthenticatedUserMappingRecord> _mappings = [];

    public InMemoryAuthenticatedUserMappingRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(AuthenticatedUserMappingRecord mapping, CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, mapping.TenantId);

        lock (_gate)
        {
            _mappings.Add(mapping);
        }

        return Task.CompletedTask;
    }

    public Task<AuthenticatedUserMappingRecord?> FindAsync(
        string externalSubjectId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult(_mappings.SingleOrDefault(mapping =>
                mapping.TenantId == tenantId
                && mapping.ExternalSubjectId.Equals(externalSubjectId, StringComparison.Ordinal)));
        }
    }
}
