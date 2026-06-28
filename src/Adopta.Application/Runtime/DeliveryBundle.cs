namespace Adopta.Application.Runtime;

public sealed record DeliveryBundle(
    Guid TenantId,
    Guid ApplicationId,
    string Environment,
    DeliveryChannel Channel,
    RuntimeContentBundle Content);

