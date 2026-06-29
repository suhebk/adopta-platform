namespace Adopta.Application.Identity;

public static class AdoptaPermissionKeys
{
    public const string DiagnosticsRead = "Diagnostics.Read";
    public const string TenantsRead = "Tenants.Read";
    public const string TenantsManage = "Tenants.Manage";
    public const string ApplicationsRead = "Applications.Read";
    public const string ApplicationsManage = "Applications.Manage";
    public const string AuditRead = "Audit.Read";
    public const string AuthoringRead = "Authoring.Read";
    public const string AuthoringManage = "Authoring.Manage";
    public const string AuthoringReview = "Authoring.Review";
    public const string AuthoringApprove = "Authoring.Approve";
    public const string AuthoringPublish = "Authoring.Publish";
    public const string RuntimeDeliveryRead = "RuntimeDelivery.Read";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        DiagnosticsRead,
        TenantsRead,
        TenantsManage,
        ApplicationsRead,
        ApplicationsManage,
        AuditRead,
        AuthoringRead,
        AuthoringManage,
        AuthoringReview,
        AuthoringApprove,
        AuthoringPublish,
        RuntimeDeliveryRead
    };
}
