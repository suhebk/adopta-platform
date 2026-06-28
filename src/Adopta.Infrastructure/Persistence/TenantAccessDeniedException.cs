namespace Adopta.Infrastructure.Persistence;

public sealed class TenantAccessDeniedException : InvalidOperationException
{
    public TenantAccessDeniedException()
        : base("Tenant access denied.")
    {
    }
}
