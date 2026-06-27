namespace Adopta.Application.Identity;

public static class AdoptaPermissionKeys
{
    public const string DiagnosticsRead = "Diagnostics.Read";
    public const string TenantsRead = "Tenants.Read";
    public const string TenantsManage = "Tenants.Manage";
    public const string ApplicationsRead = "Applications.Read";
    public const string ApplicationsManage = "Applications.Manage";
    public const string AuditRead = "Audit.Read";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        DiagnosticsRead,
        TenantsRead,
        TenantsManage,
        ApplicationsRead,
        ApplicationsManage,
        AuditRead
    };
}
