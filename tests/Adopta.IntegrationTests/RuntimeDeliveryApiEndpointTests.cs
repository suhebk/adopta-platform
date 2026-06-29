using System.Net;
using System.Net.Http.Json;
using Adopta.Api.Runtime;
using Adopta.Application.Identity;
using Adopta.Application.Runtime;
using Adopta.Domain.Identity;
using Adopta.Infrastructure.Identity;
using Adopta.Infrastructure.Runtime;
using Adopta.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class RuntimeDeliveryApiEndpointTests
{
    [Fact]
    public async Task Missing_tenant_context_is_denied()
    {
        using var factory = BuildFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/runtime/delivery/bundles/{Guid.NewGuid()}?environment=production&channel=Published");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Missing_permission_is_denied()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.DiagnosticsRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{Guid.NewGuid()}?environment=production&channel=Published",
            seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_permission_is_denied()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringPublish);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{Guid.NewGuid()}?environment=production&channel=Published",
            seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Runtime_delivery_route_requires_runtime_delivery_read_permission()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{Guid.NewGuid()}?environment=production&channel=Published",
            seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "production", "Published")]
    [InlineData("11111111-1111-1111-1111-111111111111", "", "Published")]
    [InlineData("11111111-1111-1111-1111-111111111111", "production", "")]
    [InlineData("11111111-1111-1111-1111-111111111111", "production", "Unknown")]
    public async Task Invalid_or_missing_application_environment_or_channel_fails_safely(
        string applicationId,
        string environment,
        string channel)
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{applicationId}?environment={environment}&channel={channel}",
            seed);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<RuntimeDeliveryBundleResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Succeeded);
        Assert.Equal("invalid_delivery_bundle_lookup_request", body.Status);
        Assert.Null(body.Bundle);
        Assert.NotEmpty(body.Issues);
        Assert.All(body.Issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Code));
            Assert.False(string.IsNullOrWhiteSpace(issue.Path));
            Assert.False(string.IsNullOrWhiteSpace(issue.Message));
            Assert.DoesNotContain(seed.InternalTenantId.ToString(), issue.Message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Found_bundle_returns_safe_runtime_content_contract_response()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        var applicationId = Guid.NewGuid();
        var bundle = BuildBundle(seed.InternalTenantId, applicationId, "production", DeliveryChannel.Published);
        await SeedBundleAsync(bundle);

        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{applicationId}?environment=production&channel=Published",
            seed);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<RuntimeDeliveryBundleResponse>();
        var rawBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.Succeeded);
        Assert.Equal("found", body.Status);
        Assert.NotNull(body.Bundle);
        Assert.Equal(seed.InternalTenantId, body.Bundle.TenantId);
        Assert.Equal(applicationId, body.Bundle.ApplicationId);
        Assert.Equal("production", body.Bundle.Environment);
        Assert.Equal(DeliveryChannel.Published, body.Bundle.Channel);
        Assert.Equal(bundle.Content.BundleId, body.Bundle.Content.BundleId);
        Assert.Single(body.Bundle.Content.Items);
        Assert.Empty(body.Issues);
        Assert.DoesNotContain("Bearer", rawBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", rawBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", rawBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HMRC", rawBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", rawBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Missing_bundle_returns_safe_not_found()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{Guid.NewGuid()}?environment=production&channel=Published",
            seed);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<RuntimeDeliveryBundleResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Succeeded);
        Assert.Equal("not_found", body.Status);
        Assert.Null(body.Bundle);
        Assert.Empty(body.Issues);
    }

    [Fact]
    public async Task Wrong_application_environment_or_channel_does_not_leak_other_bundles()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        var applicationId = Guid.NewGuid();
        await SeedBundleAsync(BuildBundle(seed.InternalTenantId, applicationId, "production", DeliveryChannel.Published));

        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();

        var wrongApplication = await SendLookupAsync(client, seed, Guid.NewGuid(), "production", "Published");
        var wrongEnvironment = await SendLookupAsync(client, seed, applicationId, "test", "Published");
        var wrongChannel = await SendLookupAsync(client, seed, applicationId, "production", "Preview");

        Assert.Equal(HttpStatusCode.NotFound, wrongApplication.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, wrongEnvironment.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, wrongChannel.StatusCode);
    }

    [Fact]
    public async Task Cross_tenant_lookup_is_hidden_safely()
    {
        var tenantA = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        var tenantB = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead);
        var applicationId = Guid.NewGuid();
        await SeedBundleAsync(BuildBundle(tenantB.InternalTenantId, applicationId, "production", DeliveryChannel.Published));

        using var factory = BuildFactoryWithSeed(tenantA);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{applicationId}?environment=production&channel=Published",
            tenantA);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(tenantB.InternalTenantId.ToString(), body, StringComparison.Ordinal);
        Assert.DoesNotContain(applicationId.ToString(), body, StringComparison.Ordinal);
    }

    private static async Task<HttpResponseMessage> SendLookupAsync(
        HttpClient client,
        TestIdentitySeed seed,
        Guid applicationId,
        string environment,
        string channel)
    {
        using var request = CreateAuthenticatedRequest(
            $"/runtime/delivery/bundles/{applicationId}?environment={environment}&channel={channel}",
            seed);

        return await client.SendAsync(request);
    }

    private static WebApplicationFactory<Program> BuildFactoryWithSeed(TestIdentitySeed seed)
    {
        var tenantStore = new InMemoryAdoptaTenantMappingStore();
        var userStore = new InMemoryAuthenticatedUserMappingStore();

        tenantStore.Add(seed.ExternalTenantId, seed.ApplicationId, seed.InternalTenantId);
        userStore.Add(
            seed.InternalTenantId,
            seed.SubjectId,
            BuildUser(seed.InternalTenantId, seed.SubjectId, seed.PermissionKey));

        return BuildFactory(tenantStore, userStore);
    }

    private static WebApplicationFactory<Program> BuildFactory(
        InMemoryAdoptaTenantMappingStore? tenantStore = null,
        InMemoryAuthenticatedUserMappingStore? userStore = null)
    {
        tenantStore ??= new InMemoryAdoptaTenantMappingStore();
        userStore ??= new InMemoryAuthenticatedUserMappingStore();

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Authentication:Test:Enabled", "true");

                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Authentication:Test:Enabled"] = "true"
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(tenantStore);
                    services.AddSingleton(userStore);
                });
            });
    }

    private static HttpRequestMessage CreateAuthenticatedRequest(
        string requestUri,
        TestIdentitySeed seed)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("X-Adopta-Test-Authenticated", "true");
        request.Headers.Add("X-Adopta-Test-Tid", seed.ExternalTenantId);
        request.Headers.Add("X-Adopta-Test-AppId", seed.ApplicationId);
        request.Headers.Add("X-Adopta-Test-Oid", seed.SubjectId);

        return request;
    }

    private static async Task SeedBundleAsync(DeliveryBundle bundle)
    {
        var context = new AdoptionTenantContext();
        context.SetTenant(bundle.TenantId);
        var repository = new InMemoryDeliveryBundleRepository(context);

        await repository.StoreAsync(bundle);
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
            new RuntimeContentBundle(
                Guid.NewGuid().ToString("N"),
                tenantId.ToString(),
                applicationId.ToString(),
                environment,
                channel == DeliveryChannel.Published ? "published" : "preview",
                "1.0.0",
                DateTimeOffset.UtcNow,
                [
                    new RuntimeContentItem(
                        "billing.submit",
                        RuntimeContentType.Tooltip,
                        "1.0.0",
                        "Submit return",
                        "Use this action when the return is ready.",
                        new RuntimeAnchorDescriptor("data-adopt-id", "billing.submit"),
                        new RuntimeTargetingPlaceholder("placeholder", [], []))
                ]));
    }

    private static AdoptionUser BuildUser(Guid tenantId, string subjectId, string permissionKey)
    {
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, subjectId, "Test User");
        var role = new Role(Guid.NewGuid(), tenantId, "Test Role");
        role.Grant(new Permission(permissionKey, "Test permission"));
        user.AssignRole(role);

        return user;
    }

    private sealed record TestIdentitySeed(
        string ExternalTenantId,
        string ApplicationId,
        string SubjectId,
        Guid InternalTenantId,
        string PermissionKey)
    {
        public static TestIdentitySeed Create(string permissionKey)
        {
            return new TestIdentitySeed(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid(),
                permissionKey);
        }
    }
}
