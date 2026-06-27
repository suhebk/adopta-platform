using System.Net;
using Adopta.Application.Identity;
using Adopta.Domain.Identity;
using Adopta.Infrastructure.Identity;
using Adopta.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class AuthenticationFlowTests
{
    [Fact]
    public async Task Unauthenticated_access_is_denied_for_permission_required_endpoint()
    {
        using var factory = BuildFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/diagnostics/security-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_test_scheme_access_is_allowed_when_tenant_and_user_mappings_exist()
    {
        var seed = TestIdentitySeed.Create(permissionKey: AdoptaPermissionKeys.DiagnosticsRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest("/diagnostics/security-context", seed);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(seed.InternalTenantId.ToString(), body, StringComparison.Ordinal);
        Assert.Contains("authorized", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Missing_tenant_mapping_is_denied()
    {
        var seed = TestIdentitySeed.Create(permissionKey: AdoptaPermissionKeys.DiagnosticsRead);
        using var factory = BuildFactory(userSeed: seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest("/diagnostics/tenant-context", seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Valid_tenant_mapping_allows_tenant_diagnostic_endpoint()
    {
        var seed = TestIdentitySeed.Create(permissionKey: AdoptaPermissionKeys.DiagnosticsRead);
        using var factory = BuildFactory(tenantSeed: seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest("/diagnostics/tenant-context", seed);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(seed.InternalTenantId.ToString(), body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Missing_user_role_mapping_is_denied()
    {
        var seed = TestIdentitySeed.Create(permissionKey: AdoptaPermissionKeys.DiagnosticsRead);
        using var factory = BuildFactory(tenantSeed: seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest("/diagnostics/security-context", seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Missing_permission_is_denied()
    {
        var seed = TestIdentitySeed.Create(permissionKey: AdoptaPermissionKeys.TenantsRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest("/diagnostics/security-context", seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Ambiguous_tenant_mapping_fails_closed()
    {
        var seed = TestIdentitySeed.Create(permissionKey: AdoptaPermissionKeys.DiagnosticsRead);
        var tenantStore = BuildTenantStore(seed);
        tenantStore.Add(seed.ExternalTenantId, seed.ApplicationId, Guid.NewGuid());

        using var factory = BuildFactoryWithSeed(seed, tenantStore);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest("/diagnostics/security-context", seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static WebApplicationFactory<Program> BuildFactoryWithSeed(
        TestIdentitySeed? seed = null,
        InMemoryAdoptaTenantMappingStore? tenantStore = null)
    {
        return BuildFactory(seed, seed, tenantStore);
    }

    private static WebApplicationFactory<Program> BuildFactory(
        TestIdentitySeed? tenantSeed = null,
        TestIdentitySeed? userSeed = null,
        InMemoryAdoptaTenantMappingStore? tenantStore = null)
    {
        tenantStore ??= tenantSeed is null
            ? new InMemoryAdoptaTenantMappingStore()
            : BuildTenantStore(tenantSeed);

        var userStore = new InMemoryAuthenticatedUserMappingStore();
        if (userSeed is not null)
        {
            userStore.Add(
                userSeed.InternalTenantId,
                userSeed.SubjectId,
                BuildUser(userSeed.InternalTenantId, userSeed.SubjectId, userSeed.PermissionKey));
        }

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

    private static InMemoryAdoptaTenantMappingStore BuildTenantStore(TestIdentitySeed seed)
    {
        var store = new InMemoryAdoptaTenantMappingStore();
        store.Add(seed.ExternalTenantId, seed.ApplicationId, seed.InternalTenantId);
        return store;
    }

    private static HttpRequestMessage CreateAuthenticatedRequest(string requestUri, TestIdentitySeed seed)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("X-Adopta-Test-Authenticated", "true");
        request.Headers.Add("X-Adopta-Test-Tid", seed.ExternalTenantId);
        request.Headers.Add("X-Adopta-Test-AppId", seed.ApplicationId);
        request.Headers.Add("X-Adopta-Test-Oid", seed.SubjectId);
        return request;
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
