using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Authoring;

namespace Adopta.Infrastructure.Persistence;

public sealed class InMemoryAuthoredContentLifecycleHistoryRepository
    : IAuthoredContentLifecycleHistoryRepository
{
    private readonly Lock _gate = new();
    private readonly IAdoptionTenantContext _tenantContext;
    private readonly List<AuthoredContentLifecycleAuditRecord> _records = [];

    public InMemoryAuthoredContentLifecycleHistoryRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(
        AuthoredContentLifecycleAuditRecord auditRecord,
        CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, auditRecord.TenantId);

        lock (_gate)
        {
            _records.Add(auditRecord);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<AuthoredContentLifecycleAuditRecord>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        lock (_gate)
        {
            return Task.FromResult<IReadOnlyCollection<AuthoredContentLifecycleAuditRecord>>(
                _records
                    .Where(record => record.TenantId == tenantId)
                    .OrderBy(record => record.OccurredAtUtc)
                    .ToArray());
        }
    }
}
