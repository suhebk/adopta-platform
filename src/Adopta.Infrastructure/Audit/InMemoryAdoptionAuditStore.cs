using Adopta.Domain.Audit;

namespace Adopta.Infrastructure.Audit;

public sealed class InMemoryAdoptionAuditStore
{
    private readonly Lock _gate = new();
    private readonly List<AuditEvent> _events = [];

    public void Add(AuditEvent auditEvent)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);

        lock (_gate)
        {
            _events.Add(auditEvent);
        }
    }

    public IReadOnlyCollection<AuditEvent> ListForTenant(Guid tenantId)
    {
        lock (_gate)
        {
            return _events
                .Where(auditEvent => auditEvent.TenantId == tenantId)
                .ToArray();
        }
    }
}
