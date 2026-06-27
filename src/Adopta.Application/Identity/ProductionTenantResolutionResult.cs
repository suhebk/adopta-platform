namespace Adopta.Application.Identity;

public sealed record ProductionTenantResolutionResult(
    bool IsResolved,
    Guid? TenantId,
    string? ExternalTenantId,
    string? SubjectId,
    string FailureCode)
{
    public static ProductionTenantResolutionResult Resolved(
        Guid tenantId,
        string externalTenantId,
        string? subjectId)
    {
        return new ProductionTenantResolutionResult(
            true,
            tenantId,
            externalTenantId,
            subjectId,
            string.Empty);
    }

    public static ProductionTenantResolutionResult Unresolved(string failureCode)
    {
        return new ProductionTenantResolutionResult(
            false,
            null,
            null,
            null,
            string.IsNullOrWhiteSpace(failureCode) ? "unresolved" : failureCode);
    }
}
