using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Audit;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryAuditEventRepository : IAuditEventRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<AuditEvent> _auditEvents = [];

    public InMemoryAuditEventRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, auditEvent.TenantId);

        lock (_gate)
        {
            _auditEvents.Add(auditEvent);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<AuditEvent>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<AuditEvent>>(
                _auditEvents.Where(auditEvent => auditEvent.TenantId == tenantId).ToArray());
        }
    }
}
