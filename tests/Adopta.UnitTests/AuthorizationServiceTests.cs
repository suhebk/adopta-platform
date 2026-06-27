using Adopta.Application.Security;
using Adopta.Domain.Identity;

namespace Adopta.UnitTests;

public sealed class AuthorizationServiceTests
{
    private readonly AdoptionAuthorizationService _authorization = new();

    [Fact]
    public void Allows_when_permission_exists()
    {
        var user = BuildUserWithPermission("tenant.settings.read");

        Assert.True(_authorization.HasPermission(user, "tenant.settings.read"));
    }

    [Fact]
    public void Denies_when_user_has_no_roles()
    {
        var user = new AdoptionUser(Guid.NewGuid(), Guid.NewGuid(), "entra-user-1", "Tara");

        Assert.False(_authorization.HasPermission(user, "tenant.settings.read"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Denies_when_permission_key_is_empty_or_whitespace(string permissionKey)
    {
        var user = BuildUserWithPermission("tenant.settings.read");

        Assert.False(_authorization.HasPermission(user, permissionKey));
    }

    [Fact]
    public void Denies_when_permission_casing_differs()
    {
        var user = BuildUserWithPermission("tenant.settings.read");

        Assert.False(_authorization.HasPermission(user, "Tenant.Settings.Read"));
    }

    [Fact]
    public void Duplicate_role_and_permission_grants_do_not_duplicate_effective_permissions()
    {
        var tenantId = Guid.NewGuid();
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, "entra-user-1", "Tara");
        var role = new Role(Guid.NewGuid(), tenantId, "Owner");
        var permission = new Permission("tenant.settings.read", "Read tenant settings");

        role.Grant(permission);
        role.Grant(permission);
        user.AssignRole(role);
        user.AssignRole(role);

        Assert.Single(role.Permissions);
        Assert.Single(user.Roles);
        Assert.True(_authorization.HasPermission(user, "tenant.settings.read"));
    }

    private static AdoptionUser BuildUserWithPermission(string permissionKey)
    {
        var tenantId = Guid.NewGuid();
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, "entra-user-1", "Tara");
        var role = new Role(Guid.NewGuid(), tenantId, "Owner");
        role.Grant(new Permission(permissionKey, "Test permission"));
        user.AssignRole(role);

        return user;
    }
}
