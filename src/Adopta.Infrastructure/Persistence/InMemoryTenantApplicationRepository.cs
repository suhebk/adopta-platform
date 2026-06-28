using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Tenancy;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryTenantApplicationRepository : ITenantApplicationRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<TenantApplication> _applications = [];

    public InMemoryTenantApplicationRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(TenantApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, application.TenantId);

        lock (_gate)
        {
            _applications.Add(application);
        }

        return Task.CompletedTask;
    }

    public Task<TenantApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult(_applications.SingleOrDefault(application =>
                application.Id == applicationId && application.TenantId == tenantId));
        }
    }

    public Task<IReadOnlyCollection<TenantApplication>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<TenantApplication>>(
                _applications.Where(application => application.TenantId == tenantId).ToArray());
        }
    }
}
