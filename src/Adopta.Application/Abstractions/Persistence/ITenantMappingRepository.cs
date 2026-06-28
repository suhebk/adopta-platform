using Adopta.Application.Identity;

namespace Adopta.Application.Abstractions.Persistence;

public interface ITenantMappingRepository
{
    Task AddAsync(TenantMappingRecord mapping, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantMappingRecord>> FindAsync(
        string externalTenantId,
        string applicationId,
        CancellationToken cancellationToken = default);
}
