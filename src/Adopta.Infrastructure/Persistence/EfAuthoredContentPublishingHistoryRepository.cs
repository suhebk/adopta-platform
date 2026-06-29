using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Authoring;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Persistence;

public sealed class EfAuthoredContentPublishingHistoryRepository
    : IAuthoredContentPublishingHistoryRepository
{
    private readonly AdoptaDbContext _dbContext;
    private readonly IAdoptionTenantContext _tenantContext;

    public EfAuthoredContentPublishingHistoryRepository(
        AdoptaDbContext dbContext,
        IAdoptionTenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(
        AuthoredContentPublishingAuditRecord auditRecord,
        CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, auditRecord.TenantId);

        _dbContext.AuthoredContentPublishingHistory.Add(
            AuthoredContentPublishingHistoryRecord.FromAuditRecord(auditRecord));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuthoredContentPublishingAuditRecord>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        var records = await _dbContext.AuthoredContentPublishingHistory
            .AsNoTracking()
            .Where(record => record.TenantId == tenantId)
            .OrderBy(record => record.OccurredAtUtc)
            .ToArrayAsync(cancellationToken);

        return records.Select(record => record.ToAuditRecord()).ToArray();
    }
}
