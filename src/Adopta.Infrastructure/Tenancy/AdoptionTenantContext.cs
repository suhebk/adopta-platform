using Adopta.Application.Abstractions;

namespace Adopta.Infrastructure.Tenancy;

public sealed class AdoptionTenantContext : IAdoptionTenantContext
{
    public Guid TenantId { get; private set; }

    public string? ExternalTenantId { get; private set; }

    public bool HasTenant => TenantId != Guid.Empty;

    public void SetTenant(Guid tenantId, string? externalTenantId = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        TenantId = tenantId;
        ExternalTenantId = string.IsNullOrWhiteSpace(externalTenantId)
            ? null
            : externalTenantId.Trim();
    }
}
