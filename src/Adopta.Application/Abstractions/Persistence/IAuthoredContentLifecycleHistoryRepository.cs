using Adopta.Application.Authoring;

namespace Adopta.Application.Abstractions.Persistence;

public interface IAuthoredContentLifecycleHistoryRepository
{
    Task AddAsync(AuthoredContentLifecycleAuditRecord auditRecord, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuthoredContentLifecycleAuditRecord>> ListAsync(CancellationToken cancellationToken = default);
}
