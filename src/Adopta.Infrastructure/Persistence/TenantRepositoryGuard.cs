using Adopta.Application.Abstractions;

namespace Adopta.Infrastructure.Persistence;

internal static class TenantRepositoryGuard
{
    public static Guid RequireTenant(IAdoptionTenantContext tenantContext)
    {
        if (!tenantContext.HasTenant || tenantContext.TenantId == Guid.Empty)
        {
            throw new TenantAccessDeniedException();
        }

        return tenantContext.TenantId;
    }

    public static Guid RequireTenantMatch(IAdoptionTenantContext tenantContext, Guid requestedTenantId)
    {
        var currentTenantId = RequireTenant(tenantContext);

        if (requestedTenantId == Guid.Empty || requestedTenantId != currentTenantId)
        {
            throw new TenantAccessDeniedException();
        }

        return currentTenantId;
    }
}
