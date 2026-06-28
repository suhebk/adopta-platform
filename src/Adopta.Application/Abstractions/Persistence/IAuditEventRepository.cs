using Adopta.Domain.Audit;

namespace Adopta.Application.Abstractions.Persistence;

public interface IAuditEventRepository
{
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuditEvent>> ListAsync(CancellationToken cancellationToken = default);
}
