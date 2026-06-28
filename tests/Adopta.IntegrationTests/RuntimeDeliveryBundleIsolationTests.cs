using Adopta.Application.Runtime;
using Adopta.Infrastructure.Runtime;
using Adopta.Infrastructure.Tenancy;

namespace Adopta.IntegrationTests;

public sealed class RuntimeDeliveryBundleIsolationTests
{
    [Fact]
    public async Task Matching_tenant_application_environment_and_channel_returns_bundle()
    {
        var tenantId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var repository = new InMemoryDeliveryBundleRepository(BuildTenantContext(tenantId));
        var bundle = BuildBundle(tenantId, applicationId, "production", DeliveryChannel.Published);

        await repository.StoreAsync(bundle);

        var result = await repository.LookupAsync(new DeliveryBundleLookupRequest(
            tenantId,
            applicationId,
            "production",
            DeliveryChannel.Published));

        Assert.Equal(DeliveryBundleLookupStatus.Found, result.Status);
        Assert.Same(bundle, result.Bundle);
    }

    [Fact]
    public async Task Missing_bundle_returns_safe_not_found()
    {
        var tenantId = Guid.NewGuid();
        var repository = new InMemoryDeliveryBundleRepository(BuildTenantContext(tenantId));

        var result = await repository.LookupAsync(new DeliveryBundleLookupRequest(
            tenantId,
            Guid.NewGuid(),
            "production",
            DeliveryChannel.Published));

        Assert.Equal(DeliveryBundleLookupStatus.NotFound, result.Status);
        Assert.Null(result.Bundle);
    }

    [Fact]
    public async Task Cross_tenant_lookup_is_denied_without_leaking_bundle_details()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var repository = new InMemoryDeliveryBundleRepository(BuildTenantContext(tenantA));

        var result = await repository.LookupAsync(new DeliveryBundleLookupRequest(
            tenantB,
            applicationId,
            "production",
            DeliveryChannel.Published));

        Assert.Equal(DeliveryBundleLookupStatus.AccessDenied, result.Status);
        Assert.Null(result.Bundle);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public async Task Wrong_application_environment_or_channel_does_not_leak_other_bundles()
    {
        var tenantId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var repository = new InMemoryDeliveryBundleRepository(BuildTenantContext(tenantId));

        await repository.StoreAsync(BuildBundle(tenantId, applicationId, "production", DeliveryChannel.Published));

        var wrongApplication = await repository.LookupAsync(new DeliveryBundleLookupRequest(
            tenantId,
            Guid.NewGuid(),
            "production",
            DeliveryChannel.Published));
        var wrongEnvironment = await repository.LookupAsync(new DeliveryBundleLookupRequest(
            tenantId,
            applicationId,
            "test",
            DeliveryChannel.Published));
        var wrongChannel = await repository.LookupAsync(new DeliveryBundleLookupRequest(
            tenantId,
            applicationId,
            "production",
            DeliveryChannel.Preview));

        Assert.Equal(DeliveryBundleLookupStatus.NotFound, wrongApplication.Status);
        Assert.Equal(DeliveryBundleLookupStatus.NotFound, wrongEnvironment.Status);
        Assert.Equal(DeliveryBundleLookupStatus.NotFound, wrongChannel.Status);
    }

    [Fact]
    public async Task Invalid_bundle_returns_validation_failed()
    {
        var tenantId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var repository = new InMemoryDeliveryBundleRepository(BuildTenantContext(tenantId));
        var bundle = BuildBundle(tenantId, applicationId, "production", DeliveryChannel.Published) with
        {
            Content = BuildContentBundle(tenantId, applicationId, "production", "preview")
        };

        var result = await repository.StoreAsync(bundle);

        Assert.Equal(DeliveryBundleLookupStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_delivery_bundle");
    }

    private static AdoptionTenantContext BuildTenantContext(Guid tenantId)
    {
        var context = new AdoptionTenantContext();
        context.SetTenant(tenantId);
        return context;
    }

    private static DeliveryBundle BuildBundle(
        Guid tenantId,
        Guid applicationId,
        string environment,
        DeliveryChannel channel)
    {
        return new DeliveryBundle(
            tenantId,
            applicationId,
            environment,
            channel,
            BuildContentBundle(tenantId, applicationId, environment, channel == DeliveryChannel.Published ? "published" : "preview"));
    }

    private static RuntimeContentBundle BuildContentBundle(
        Guid tenantId,
        Guid applicationId,
        string environment,
        string channel)
    {
        return new RuntimeContentBundle(
            Guid.NewGuid().ToString(),
            tenantId.ToString(),
            applicationId.ToString(),
            environment,
            channel,
            "2026.06.28",
            DateTimeOffset.UtcNow,
            [
                new RuntimeContentItem(
                    "item-1",
                    RuntimeContentType.Tooltip,
                    "1.0.0",
                    "Submit return",
                    "Use this action when the return is ready.",
                    new RuntimeAnchorDescriptor("data-adopt-id", "billing.submit"),
                    new RuntimeTargetingPlaceholder("placeholder", [], []))
            ]);
    }
}
