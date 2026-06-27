namespace Adopta.Domain.Common;

public abstract class TenantScopedEntity : ITenantScopedEntity
{
    protected TenantScopedEntity(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}
