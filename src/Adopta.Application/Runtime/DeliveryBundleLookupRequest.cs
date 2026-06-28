namespace Adopta.Application.Runtime;

public sealed record DeliveryBundleLookupRequest(
    Guid TenantId,
    Guid ApplicationId,
    string Environment,
    DeliveryChannel Channel)
{
    public IReadOnlyCollection<RuntimeContentValidationIssue> Validate()
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (TenantId == Guid.Empty)
        {
            issues.Add(new RuntimeContentValidationIssue(
                "invalid_delivery_bundle_lookup_request",
                "request.tenantId",
                "Tenant id is required."));
        }

        if (ApplicationId == Guid.Empty)
        {
            issues.Add(new RuntimeContentValidationIssue(
                "invalid_delivery_bundle_lookup_request",
                "request.applicationId",
                "Application id is required."));
        }

        if (string.IsNullOrWhiteSpace(Environment))
        {
            issues.Add(new RuntimeContentValidationIssue(
                "invalid_delivery_bundle_lookup_request",
                "request.environment",
                "Environment is required."));
        }

        return issues;
    }
}

