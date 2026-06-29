namespace Adopta.Infrastructure.Persistence;

public sealed class AuthenticatedUserMappingEntity
{
    private AuthenticatedUserMappingEntity()
    {
        ExternalSubjectId = string.Empty;
    }

    public AuthenticatedUserMappingEntity(Guid tenantId, string externalSubjectId, Guid userId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        TenantId = tenantId;
        ExternalSubjectId = string.IsNullOrWhiteSpace(externalSubjectId)
            ? throw new ArgumentException("External subject id is required.", nameof(externalSubjectId))
            : externalSubjectId.Trim();
        UserId = userId;
    }

    public Guid TenantId { get; private set; }

    public string ExternalSubjectId { get; private set; }

    public Guid UserId { get; private set; }
}
