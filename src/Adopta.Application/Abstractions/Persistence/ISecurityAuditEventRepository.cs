using Adopta.Application.Audit;

namespace Adopta.Application.Abstractions.Persistence;

public interface ISecurityAuditEventRepository
{
    Task AddAsync(SecurityAuditEventRecord auditEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SecurityAuditEventRecord>> ListAsync(CancellationToken cancellationToken = default);
}
