using Adopta.Api.Auth;
using Adopta.Application.Abstractions;
using Adopta.Application.Identity;
using Adopta.Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class PermissionEndpointFilterTests
{
    [Fact]
    public async Task Allows_when_tenant_context_and_permission_are_present()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.DiagnosticsRead);
        var context = BuildInvocationContext(hasTenant: true, user, isAllowed: true);
        var filter = new RequirePermissionEndpointFilter(AdoptaPermissionKeys.DiagnosticsRead);

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("allowed"));

        Assert.Equal("allowed", result);
    }

    [Fact]
    public async Task Denies_when_tenant_context_is_missing()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.DiagnosticsRead);
        var context = BuildInvocationContext(hasTenant: false, user, isAllowed: true);
        var filter = new RequirePermissionEndpointFilter(AdoptaPermissionKeys.DiagnosticsRead);

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("allowed"));

        Assert.NotEqual("allowed", result);
    }

    [Fact]
    public async Task Denies_when_permission_key_is_empty()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.DiagnosticsRead);
        var context = BuildInvocationContext(hasTenant: true, user, isAllowed: true);
        var filter = new RequirePermissionEndpointFilter(string.Empty);

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("allowed"));

        Assert.NotEqual("allowed", result);
    }

    [Fact]
    public async Task Denies_when_permission_is_not_granted()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.TenantsRead);
        var context = BuildInvocationContext(hasTenant: true, user, isAllowed: false);
        var filter = new RequirePermissionEndpointFilter(AdoptaPermissionKeys.DiagnosticsRead);

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("allowed"));

        Assert.NotEqual("allowed", result);
    }

    private static EndpointFilterInvocationContext BuildInvocationContext(
        bool hasTenant,
        AdoptionUser user,
        bool isAllowed)
    {
        var services = new ServiceCollection()
            .AddSingleton<IAdoptionTenantContext>(new TestTenantContext(hasTenant))
            .AddSingleton<IAdoptionPermissionEvaluator>(new TestPermissionEvaluator(isAllowed))
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };
        httpContext.Items[RequirePermissionEndpointFilter.CurrentUserItemName] = user;

        return new DefaultEndpointFilterInvocationContext(httpContext, []);
    }

    private static AdoptionUser BuildUserWithPermission(string permissionKey)
    {
        var tenantId = Guid.NewGuid();
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, "entra-user-1", "Tara");
        var role = new Role(Guid.NewGuid(), tenantId, "Test");
        role.Grant(new Permission(permissionKey, "Test permission"));
        user.AssignRole(role);

        return user;
    }

    private sealed class TestTenantContext : IAdoptionTenantContext
    {
        public TestTenantContext(bool hasTenant)
        {
            HasTenant = hasTenant;
            TenantId = hasTenant ? Guid.NewGuid() : Guid.Empty;
        }

        public Guid TenantId { get; }

        public string? ExternalTenantId => null;

        public bool HasTenant { get; }
    }

    private sealed class TestPermissionEvaluator : IAdoptionPermissionEvaluator
    {
        private readonly bool _isAllowed;

        public TestPermissionEvaluator(bool isAllowed)
        {
            _isAllowed = isAllowed;
        }

        public bool IsAllowed(AdoptionUser? user, string permissionKey)
        {
            return _isAllowed;
        }
    }
}
