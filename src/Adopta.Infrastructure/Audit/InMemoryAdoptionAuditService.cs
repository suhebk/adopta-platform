using Adopta.Application.Abstractions;
using Adopta.Domain.Audit;

namespace Adopta.Infrastructure.Audit;

public sealed class InMemoryAdoptionAuditService : IAdoptionAuditService
{
    private readonly InMemoryAdoptionAuditStore _store;
    private readonly IAdoptionTenantContext _tenantContext;

    public InMemoryAdoptionAuditService(
        InMemoryAdoptionAuditStore store,
        IAdoptionTenantContext tenantContext)
    {
        _store = store;
        _tenantContext = tenantContext;
    }

    public Task<AuditEvent> RecordAsync(
        Guid actorUserId,
        string action,
        string targetType,
        string targetId,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            throw new InvalidOperationException("Tenant context is required to record audit events.");
        }

        var auditEvent = new AuditEvent(
            Guid.NewGuid(),
            _tenantContext.TenantId,
            actorUserId,
            action,
            targetType,
            targetId,
            DateTimeOffset.UtcNow);

        _store.Add(auditEvent);

        return Task.FromResult(auditEvent);
    }
}
