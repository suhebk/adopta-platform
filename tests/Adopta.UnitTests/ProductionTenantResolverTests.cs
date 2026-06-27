using System.Security.Claims;
using Adopta.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace Adopta.UnitTests;

public sealed class ProductionTenantResolverTests
{
    [Fact]
    public void Fails_closed_when_principal_is_not_authenticated()
    {
        var resolver = BuildResolver();

        var result = resolver.Resolve(new ClaimsPrincipal());

        Assert.False(result.IsResolved);
        Assert.Equal("principal_not_authenticated", result.FailureCode);
    }

    [Fact]
    public void Fails_closed_when_entra_tenant_claim_is_missing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("oid", Guid.NewGuid().ToString())],
            authenticationType: "Test"));
        var resolver = BuildResolver();

        var result = resolver.Resolve(principal);

        Assert.False(result.IsResolved);
        Assert.Equal("tenant_claim_missing", result.FailureCode);
    }

    [Fact]
    public void Resolves_when_expected_validated_claims_are_present()
    {
        var tenantId = Guid.NewGuid();
        var subjectId = Guid.NewGuid().ToString();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("tid", tenantId.ToString()),
                new Claim("oid", subjectId)
            ],
            authenticationType: "Test"));
        var resolver = BuildResolver();

        var result = resolver.Resolve(principal);

        Assert.True(result.IsResolved);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(tenantId.ToString(), result.ExternalTenantId);
        Assert.Equal(subjectId, result.SubjectId);
    }

    private static ClaimsPrincipalProductionTenantResolver BuildResolver()
    {
        return new ClaimsPrincipalProductionTenantResolver(
            Options.Create(new EntraTenantResolutionOptions()));
    }
}
