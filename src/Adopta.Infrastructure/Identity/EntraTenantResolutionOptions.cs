namespace Adopta.Infrastructure.Identity;

public sealed class EntraTenantResolutionOptions
{
    public const string SectionName = "Authentication:EntraTenantResolution";

    public string TenantIdClaimType { get; set; } = "tid";

    public string SubjectClaimType { get; set; } = "oid";
}
