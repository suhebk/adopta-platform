using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Runtime;
using Adopta.Application.Runtime;
using Adopta.Infrastructure.Persistence;

namespace Adopta.Infrastructure.Runtime;

public sealed class InMemoryDeliveryBundleRepository : IDeliveryBundleRepository
{
    private static readonly List<DeliveryBundle> Bundles = [];
    private static readonly object Gate = new();
    private readonly IAdoptionTenantContext _tenantContext;

    public InMemoryDeliveryBundleRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task<DeliveryBundleLookupResult> StoreAsync(
        DeliveryBundle bundle,
        CancellationToken cancellationToken = default)
    {
        try
        {
            TenantRepositoryGuard.RequireTenantMatch(_tenantContext, bundle.TenantId);
        }
        catch (TenantAccessDeniedException)
        {
            return Task.FromResult(DeliveryBundleLookupResult.AccessDenied());
        }

        var validationIssues = ValidateBundle(bundle);
        if (validationIssues.Count > 0)
        {
            return Task.FromResult(DeliveryBundleLookupResult.ValidationFailed(validationIssues));
        }

        lock (Gate)
        {
            Bundles.RemoveAll(existing => SameScope(existing, bundle));
            Bundles.Add(bundle);
        }

        return Task.FromResult(DeliveryBundleLookupResult.Found(bundle));
    }

    public Task<DeliveryBundleLookupResult> LookupAsync(
        DeliveryBundleLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestIssues = request.Validate();
        if (requestIssues.Count > 0)
        {
            return Task.FromResult(DeliveryBundleLookupResult.InvalidRequest(requestIssues));
        }

        try
        {
            TenantRepositoryGuard.RequireTenantMatch(_tenantContext, request.TenantId);
        }
        catch (TenantAccessDeniedException)
        {
            return Task.FromResult(DeliveryBundleLookupResult.AccessDenied());
        }

        DeliveryBundle? bundle;
        lock (Gate)
        {
            bundle = Bundles
                .Where(candidate =>
                    candidate.TenantId == request.TenantId &&
                    candidate.ApplicationId == request.ApplicationId &&
                    string.Equals(candidate.Environment, request.Environment, StringComparison.Ordinal) &&
                    candidate.Channel == request.Channel)
                .OrderByDescending(candidate => candidate.Content.GeneratedAtUtc)
                .FirstOrDefault();
        }

        return Task.FromResult(bundle is null
            ? DeliveryBundleLookupResult.NotFound()
            : DeliveryBundleLookupResult.Found(bundle));
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateBundle(DeliveryBundle bundle)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (bundle.TenantId == Guid.Empty)
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.tenantId", "Tenant id is required."));
        }

        if (bundle.ApplicationId == Guid.Empty)
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.applicationId", "Application id is required."));
        }

        if (string.IsNullOrWhiteSpace(bundle.Environment))
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.environment", "Environment is required."));
        }

        issues.AddRange(RuntimeContentValidator.ValidateBundle(bundle.Content));

        if (bundle.Content.TenantId != bundle.TenantId.ToString())
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.content.tenantId", "Content tenant id must match delivery scope."));
        }

        if (bundle.Content.ApplicationId != bundle.ApplicationId.ToString())
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.content.applicationId", "Content application id must match delivery scope."));
        }

        if (!string.Equals(bundle.Content.Environment, bundle.Environment, StringComparison.Ordinal))
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.content.environment", "Content environment must match delivery scope."));
        }

        if (!string.Equals(bundle.Content.Channel, ToContractChannel(bundle.Channel), StringComparison.Ordinal))
        {
            issues.Add(new RuntimeContentValidationIssue("invalid_delivery_bundle", "bundle.content.channel", "Content channel must match delivery scope."));
        }

        return issues;
    }

    private static bool SameScope(DeliveryBundle left, DeliveryBundle right)
    {
        return left.TenantId == right.TenantId &&
            left.ApplicationId == right.ApplicationId &&
            string.Equals(left.Environment, right.Environment, StringComparison.Ordinal) &&
            left.Channel == right.Channel;
    }

    private static string ToContractChannel(DeliveryChannel channel)
    {
        return channel switch
        {
            DeliveryChannel.Preview => "preview",
            DeliveryChannel.Published => "published",
            _ => string.Empty
        };
    }
}

