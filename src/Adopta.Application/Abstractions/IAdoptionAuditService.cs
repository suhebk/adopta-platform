using Adopta.Domain.Audit;

namespace Adopta.Application.Abstractions;

public interface IAdoptionAuditService
{
    Task<AuditEvent> RecordAsync(
        Guid actorUserId,
        string action,
        string targetType,
        string targetId,
        CancellationToken cancellationToken = default);
}
