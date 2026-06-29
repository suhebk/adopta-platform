using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Persistence;

public sealed class EfTenantMappingRepository : ITenantMappingRepository
{
    private readonly AdoptaDbContext _dbContext;
    private readonly IAdoptionTenantContext _tenantContext;

    public EfTenantMappingRepository(
        AdoptaDbContext dbContext,
        IAdoptionTenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(TenantMappingRecord mapping, CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, mapping.TenantId);

        _dbContext.TenantMappings.Add(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantMappingRecord>> FindAsync(
        string externalTenantId,
        string applicationId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        return await _dbContext.TenantMappings
            .AsNoTracking()
            .Where(mapping =>
                mapping.TenantId == tenantId
                && mapping.ExternalTenantId == externalTenantId
                && mapping.ApplicationId == applicationId)
            .ToArrayAsync(cancellationToken);
    }
}
