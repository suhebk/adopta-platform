using Adopta.Application.Abstractions;
using Adopta.Domain.Tenancy;

namespace Adopta.Infrastructure.Tenancy;

public sealed class InMemoryApplicationRegistrationService : IApplicationRegistrationService
{
    private readonly InMemoryTenantScopedStore<TenantApplication> _store;
    private readonly IAdoptionTenantContext _tenantContext;

    public InMemoryApplicationRegistrationService(
        InMemoryTenantScopedStore<TenantApplication> store,
        IAdoptionTenantContext tenantContext)
    {
        _store = store;
        _tenantContext = tenantContext;
    }

    public Task<TenantApplication> RegisterAsync(
        string name,
        Uri allowedOrigin,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            throw new InvalidOperationException("Tenant context is required to register applications.");
        }

        var application = new TenantApplication(
            Guid.NewGuid(),
            _tenantContext.TenantId,
            name,
            allowedOrigin);

        _store.Add(application);

        return Task.FromResult(application);
    }
}
