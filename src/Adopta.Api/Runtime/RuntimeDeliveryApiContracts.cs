using Adopta.Application.Runtime;

namespace Adopta.Api.Runtime;

public sealed record RuntimeDeliveryBundleResponse(
    bool Succeeded,
    string Status,
    DeliveryBundleResponse? Bundle,
    IReadOnlyCollection<RuntimeDeliveryIssueResponse> Issues);

public sealed record DeliveryBundleResponse(
    Guid TenantId,
    Guid ApplicationId,
    string Environment,
    DeliveryChannel Channel,
    RuntimeContentBundle Content);

public sealed record RuntimeDeliveryIssueResponse(
    string Code,
    string Path,
    string Message);
