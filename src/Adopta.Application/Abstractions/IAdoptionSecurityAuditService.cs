namespace Adopta.Application.Abstractions;

public interface IAdoptionSecurityAuditService
{
    Task RecordAsync(
        string action,
        string outcome,
        Guid? tenantId = null,
        string? failureCategory = null,
        CancellationToken cancellationToken = default);
}
