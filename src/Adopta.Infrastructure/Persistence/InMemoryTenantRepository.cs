using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Tenancy;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryTenantRepository : ITenantRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<Tenant> _tenants = [];

    public InMemoryTenantRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, tenant.Id);

        lock (_gate)
        {
            _tenants.Add(tenant);
        }

        return Task.CompletedTask;
    }

    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, tenantId);

        lock (_gate)
        {
            return Task.FromResult(_tenants.SingleOrDefault(tenant => tenant.Id == tenantId));
        }
    }
}
