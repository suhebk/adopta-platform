using System.Security.Claims;
using Adopta.Infrastructure.Tenancy;
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
    public void Fails_closed_when_application_claim_is_missing()
    {
        var externalTenantId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("tid", externalTenantId.ToString()),
                new Claim("oid", Guid.NewGuid().ToString())
            ],
            authenticationType: "Test"));
        var resolver = BuildResolver();

        var result = resolver.Resolve(principal);

        Assert.False(result.IsResolved);
        Assert.Equal("application_claim_missing", result.FailureCode);
    }

    [Fact]
    public void Resolves_when_expected_validated_claims_map_to_internal_tenant()
    {
        var internalTenantId = Guid.NewGuid();
        var externalTenantId = Guid.NewGuid();
        var applicationId = Guid.NewGuid().ToString();
        var subjectId = Guid.NewGuid().ToString();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("tid", externalTenantId.ToString()),
                new Claim("azp", applicationId),
                new Claim("oid", subjectId)
            ],
            authenticationType: "Test"));
        var store = new InMemoryAdoptaTenantMappingStore();
        store.Add(externalTenantId.ToString(), applicationId, internalTenantId);
        var resolver = BuildResolver(store);

        var result = resolver.Resolve(principal);

        Assert.True(result.IsResolved);
        Assert.Equal(internalTenantId, result.TenantId);
        Assert.Equal(externalTenantId.ToString(), result.ExternalTenantId);
        Assert.Equal(subjectId, result.SubjectId);
    }

    private static ClaimsPrincipalProductionTenantResolver BuildResolver(
        InMemoryAdoptaTenantMappingStore? store = null)
    {
        store ??= new InMemoryAdoptaTenantMappingStore();
        return new ClaimsPrincipalProductionTenantResolver(
            new InMemoryAdoptaTenantMappingService(store),
            Options.Create(new EntraTenantResolutionOptions()));
    }
}
