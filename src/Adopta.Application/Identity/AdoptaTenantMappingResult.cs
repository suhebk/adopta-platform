namespace Adopta.Application.Identity;

public sealed record AdoptaTenantMappingResult(
    bool IsMapped,
    Guid? TenantId,
    string FailureCode)
{
    public static AdoptaTenantMappingResult Mapped(Guid tenantId)
    {
        return new AdoptaTenantMappingResult(true, tenantId, string.Empty);
    }

    public static AdoptaTenantMappingResult Unmapped(string failureCode)
    {
        return new AdoptaTenantMappingResult(
            false,
            null,
            string.IsNullOrWhiteSpace(failureCode) ? "tenant_mapping_failed" : failureCode);
    }
}
