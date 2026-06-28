using Adopta.Application.Runtime;

namespace Adopta.Application.Abstractions.Runtime;

public interface IDeliveryBundleRepository
{
    Task<DeliveryBundleLookupResult> StoreAsync(
        DeliveryBundle bundle,
        CancellationToken cancellationToken = default);

    Task<DeliveryBundleLookupResult> LookupAsync(
        DeliveryBundleLookupRequest request,
        CancellationToken cancellationToken = default);
}

