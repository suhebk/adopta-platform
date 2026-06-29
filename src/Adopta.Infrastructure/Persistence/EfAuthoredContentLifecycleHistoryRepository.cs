using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Authoring;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Persistence;

public sealed class EfAuthoredContentLifecycleHistoryRepository
    : IAuthoredContentLifecycleHistoryRepository
{
    private readonly AdoptaDbContext _dbContext;
    private readonly IAdoptionTenantContext _tenantContext;

    public EfAuthoredContentLifecycleHistoryRepository(
        AdoptaDbContext dbContext,
        IAdoptionTenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(
        AuthoredContentLifecycleAuditRecord auditRecord,
        CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, auditRecord.TenantId);

        _dbContext.AuthoredContentLifecycleHistory.Add(
            AuthoredContentLifecycleHistoryRecord.FromAuditRecord(auditRecord));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuthoredContentLifecycleAuditRecord>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        var records = await _dbContext.AuthoredContentLifecycleHistory
            .AsNoTracking()
            .Where(record => record.TenantId == tenantId)
            .OrderBy(record => record.OccurredAtUtc)
            .ToArrayAsync(cancellationToken);

        return records.Select(record => record.ToAuditRecord()).ToArray();
    }
}
