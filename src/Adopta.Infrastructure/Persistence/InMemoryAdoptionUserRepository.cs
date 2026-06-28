using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Identity;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryAdoptionUserRepository : IAdoptionUserRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<AdoptionUser> _users = [];

    public InMemoryAdoptionUserRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(AdoptionUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, user.TenantId);

        lock (_gate)
        {
            _users.Add(user);
        }

        return Task.CompletedTask;
    }

    public Task<AdoptionUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult(_users.SingleOrDefault(user => user.Id == userId && user.TenantId == tenantId));
        }
    }

    public Task<IReadOnlyCollection<AdoptionUser>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<AdoptionUser>>(
                _users.Where(user => user.TenantId == tenantId).ToArray());
        }
    }
}
