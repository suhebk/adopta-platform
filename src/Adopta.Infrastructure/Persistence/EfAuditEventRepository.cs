using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Audit;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Persistence;

public sealed class EfAuditEventRepository : IAuditEventRepository
{
    private readonly AdoptaDbContext _dbContext;
    private readonly IAdoptionTenantContext _tenantContext;

    public EfAuditEventRepository(
        AdoptaDbContext dbContext,
        IAdoptionTenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, auditEvent.TenantId);

        _dbContext.AuditEvents.Add(auditEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditEvent>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        return await _dbContext.AuditEvents
            .AsNoTracking()
            .Where(auditEvent => auditEvent.TenantId == tenantId)
            .OrderBy(auditEvent => auditEvent.OccurredAtUtc)
            .ToArrayAsync(cancellationToken);
    }
}
