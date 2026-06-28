using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Audit;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemorySecurityAuditEventRepository : ISecurityAuditEventRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<SecurityAuditEventRecord> _auditEvents = [];

    public InMemorySecurityAuditEventRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(SecurityAuditEventRecord auditEvent, CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, auditEvent.TenantId);

        lock (_gate)
        {
            _auditEvents.Add(auditEvent);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<SecurityAuditEventRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<SecurityAuditEventRecord>>(
                _auditEvents.Where(auditEvent => auditEvent.TenantId == tenantId).ToArray());
        }
    }
}
