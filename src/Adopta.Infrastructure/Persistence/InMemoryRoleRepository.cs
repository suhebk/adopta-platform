using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Identity;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryRoleRepository : IRoleRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<Role> _roles = [];

    public InMemoryRoleRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, role.TenantId);

        lock (_gate)
        {
            _roles.Add(role);
        }

        return Task.CompletedTask;
    }

    public Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult(_roles.SingleOrDefault(role => role.Id == roleId && role.TenantId == tenantId));
        }
    }

    public Task<IReadOnlyCollection<Role>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<Role>>(
                _roles.Where(role => role.TenantId == tenantId).ToArray());
        }
    }
}
