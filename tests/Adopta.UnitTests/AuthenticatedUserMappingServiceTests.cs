using System.Security.Claims;
using Adopta.Application.Identity;
using Adopta.Domain.Identity;
using Adopta.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace Adopta.UnitTests;

public sealed class AuthenticatedUserMappingServiceTests
{
    [Fact]
    public void Maps_authenticated_subject_to_user_roles()
    {
        var tenantId = Guid.NewGuid();
        var subjectId = Guid.NewGuid().ToString();
        var user = BuildUser(tenantId, AdoptaPermissionKeys.DiagnosticsRead);
        var store = new InMemoryAuthenticatedUserMappingStore();
        store.Add(tenantId, subjectId, user);
        var service = BuildService(store);
        var principal = BuildPrincipal(subjectId);

        var result = service.MapUser(tenantId, principal);

        Assert.True(result.IsMapped);
        Assert.Same(user, result.User);
    }

    [Fact]
    public void Fails_closed_when_subject_claim_is_missing()
    {
        var service = BuildService(new InMemoryAuthenticatedUserMappingStore());
        var principal = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: "Test"));

        var result = service.MapUser(Guid.NewGuid(), principal);

        Assert.False(result.IsMapped);
        Assert.Equal("subject_claim_missing", result.FailureCode);
    }

    [Fact]
    public void Fails_closed_when_user_mapping_is_missing()
    {
        var service = BuildService(new InMemoryAuthenticatedUserMappingStore());
        var principal = BuildPrincipal(Guid.NewGuid().ToString());

        var result = service.MapUser(Guid.NewGuid(), principal);

        Assert.False(result.IsMapped);
        Assert.Equal("user_mapping_not_found", result.FailureCode);
    }

    private static InMemoryAuthenticatedUserMappingService BuildService(InMemoryAuthenticatedUserMappingStore store)
    {
        return new InMemoryAuthenticatedUserMappingService(
            store,
            Options.Create(new EntraTenantResolutionOptions()));
    }

    private static ClaimsPrincipal BuildPrincipal(string subjectId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("oid", subjectId)],
            authenticationType: "Test"));
    }

    private static AdoptionUser BuildUser(Guid tenantId, string permissionKey)
    {
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, "external-user", "Tara");
        var role = new Role(Guid.NewGuid(), tenantId, "Diagnostics");
        role.Grant(new Permission(permissionKey, "Diagnostics permission"));
        user.AssignRole(role);

        return user;
    }
}
