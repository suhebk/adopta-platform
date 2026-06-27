namespace Adopta.Infrastructure.Audit;

public sealed class InMemoryAdoptionSecurityAuditStore
{
    private readonly Lock _gate = new();
    private readonly List<InMemorySecurityAuditEvent> _events = [];

    public void Add(InMemorySecurityAuditEvent auditEvent)
    {
        lock (_gate)
        {
            _events.Add(auditEvent);
        }
    }

    public IReadOnlyCollection<InMemorySecurityAuditEvent> List()
    {
        lock (_gate)
        {
            return _events.ToArray();
        }
    }
}
