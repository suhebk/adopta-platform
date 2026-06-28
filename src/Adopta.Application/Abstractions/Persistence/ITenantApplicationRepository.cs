using Adopta.Domain.Tenancy;

namespace Adopta.Application.Abstractions.Persistence;

public interface ITenantApplicationRepository
{
    Task AddAsync(TenantApplication application, CancellationToken cancellationToken = default);

    Task<TenantApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantApplication>> ListAsync(CancellationToken cancellationToken = default);
}
