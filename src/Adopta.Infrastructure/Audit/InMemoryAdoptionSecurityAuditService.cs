using Adopta.Application.Abstractions;

namespace Adopta.Infrastructure.Audit;

public sealed class InMemoryAdoptionSecurityAuditService : IAdoptionSecurityAuditService
{
    private readonly InMemoryAdoptionSecurityAuditStore _store;

    public InMemoryAdoptionSecurityAuditService(InMemoryAdoptionSecurityAuditStore store)
    {
        _store = store;
    }

    public Task RecordAsync(
        string action,
        string outcome,
        Guid? tenantId = null,
        string? failureCategory = null,
        CancellationToken cancellationToken = default)
    {
        _store.Add(new InMemorySecurityAuditEvent(
            DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(action) ? "unknown" : action.Trim(),
            string.IsNullOrWhiteSpace(outcome) ? "unknown" : outcome.Trim(),
            tenantId,
            string.IsNullOrWhiteSpace(failureCategory) ? null : failureCategory.Trim()));

        return Task.CompletedTask;
    }
}
