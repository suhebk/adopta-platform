using Adopta.Application.Abstractions;
using Adopta.Application.Identity;
using Adopta.Application.Security;
using Adopta.Domain.Identity;

namespace Adopta.UnitTests;

public sealed class PermissionEvaluatorTests
{
    [Fact]
    public void Allows_when_permission_is_granted()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.DiagnosticsRead);
        var evaluator = new AdoptionPermissionEvaluator(new AdoptionAuthorizationService());

        Assert.True(evaluator.IsAllowed(user, AdoptaPermissionKeys.DiagnosticsRead));
    }

    [Fact]
    public void Denies_when_permission_is_not_granted()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.TenantsRead);
        var evaluator = new AdoptionPermissionEvaluator(new AdoptionAuthorizationService());

        Assert.False(evaluator.IsAllowed(user, AdoptaPermissionKeys.DiagnosticsRead));
    }

    [Fact]
    public void Denies_when_permission_key_is_empty()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.DiagnosticsRead);
        var evaluator = new AdoptionPermissionEvaluator(new AdoptionAuthorizationService());

        Assert.False(evaluator.IsAllowed(user, string.Empty));
    }

    [Fact]
    public void Denies_when_authorization_service_fails()
    {
        var user = BuildUserWithPermission(AdoptaPermissionKeys.DiagnosticsRead);
        var evaluator = new AdoptionPermissionEvaluator(new ThrowingAuthorizationService());

        Assert.False(evaluator.IsAllowed(user, AdoptaPermissionKeys.DiagnosticsRead));
    }

    private static AdoptionUser BuildUserWithPermission(string permissionKey)
    {
        var tenantId = Guid.NewGuid();
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, "entra-user-1", "Tara");
        var role = new Role(Guid.NewGuid(), tenantId, "Diagnostics");
        role.Grant(new Permission(permissionKey, "Test permission"));
        user.AssignRole(role);

        return user;
    }

    private sealed class ThrowingAuthorizationService : IAdoptionAuthorizationService
    {
        public bool HasPermission(AdoptionUser user, string permissionKey)
        {
            throw new InvalidOperationException("Simulated authorization failure.");
        }
    }
}
