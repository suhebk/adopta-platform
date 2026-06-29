using Adopta.Application.Authoring;

namespace Adopta.Application.Abstractions.Persistence;

public interface IAuthoredContentPublishingHistoryRepository
{
    Task AddAsync(AuthoredContentPublishingAuditRecord auditRecord, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuthoredContentPublishingAuditRecord>> ListAsync(CancellationToken cancellationToken = default);
}
