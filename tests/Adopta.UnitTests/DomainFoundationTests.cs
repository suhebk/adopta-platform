using Adopta.Application.Security;
using Adopta.Domain.Identity;
using Adopta.Domain.Tenancy;

namespace Adopta.UnitTests;

public sealed class DomainFoundationTests
{
    [Fact]
    public void Tenant_requires_non_empty_identity()
    {
        Assert.Throws<ArgumentException>(() => new Tenant(Guid.Empty, "Contoso", "contoso.com", "UK"));
    }

    [Fact]
    public void User_cannot_be_assigned_role_from_another_tenant()
    {
        var user = new AdoptionUser(Guid.NewGuid(), Guid.NewGuid(), "entra-user-1", "Tara");
        var otherTenantRole = new Role(Guid.NewGuid(), Guid.NewGuid(), "Owner");

        Assert.Throws<InvalidOperationException>(() => user.AssignRole(otherTenantRole));
    }

    [Fact]
    public void Authorization_service_resolves_permissions_from_assigned_roles()
    {
        var tenantId = Guid.NewGuid();
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, "entra-user-1", "Tara");
        var owner = new Role(Guid.NewGuid(), tenantId, "Owner");
        owner.Grant(new Permission("tenant.settings.read", "Read tenant settings"));
        user.AssignRole(owner);

        var authorization = new AdoptionAuthorizationService();

        Assert.True(authorization.HasPermission(user, "tenant.settings.read"));
        Assert.False(authorization.HasPermission(user, "tenant.settings.write"));
    }
}
