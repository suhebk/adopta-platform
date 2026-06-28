namespace Adopta.Application.Runtime;

public enum DeliveryBundleLookupStatus
{
    Found,
    InvalidRequest,
    NotFound,
    AccessDenied,
    ValidationFailed
}

public sealed record DeliveryBundleLookupResult(
    DeliveryBundleLookupStatus Status,
    DeliveryBundle? Bundle,
    IReadOnlyCollection<RuntimeContentValidationIssue> Issues)
{
    public static DeliveryBundleLookupResult Found(DeliveryBundle bundle)
    {
        return new DeliveryBundleLookupResult(DeliveryBundleLookupStatus.Found, bundle, []);
    }

    public static DeliveryBundleLookupResult InvalidRequest(IReadOnlyCollection<RuntimeContentValidationIssue> issues)
    {
        return new DeliveryBundleLookupResult(DeliveryBundleLookupStatus.InvalidRequest, null, issues);
    }

    public static DeliveryBundleLookupResult NotFound()
    {
        return new DeliveryBundleLookupResult(DeliveryBundleLookupStatus.NotFound, null, []);
    }

    public static DeliveryBundleLookupResult AccessDenied()
    {
        return new DeliveryBundleLookupResult(DeliveryBundleLookupStatus.AccessDenied, null, []);
    }

    public static DeliveryBundleLookupResult ValidationFailed(IReadOnlyCollection<RuntimeContentValidationIssue> issues)
    {
        return new DeliveryBundleLookupResult(DeliveryBundleLookupStatus.ValidationFailed, null, issues);
    }
}

