using System.Security.Claims;
using Adopta.Application.Abstractions;
using Adopta.Application.Identity;
using Microsoft.Extensions.Options;

namespace Adopta.Infrastructure.Identity;

public sealed class ClaimsPrincipalProductionTenantResolver : IProductionTenantResolver
{
    private readonly EntraTenantResolutionOptions _options;

    public ClaimsPrincipalProductionTenantResolver(IOptions<EntraTenantResolutionOptions> options)
    {
        _options = options.Value;
    }

    public ProductionTenantResolutionResult Resolve(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true)
        {
            return ProductionTenantResolutionResult.Unresolved("principal_not_authenticated");
        }

        var externalTenantId = principal.FindFirst(_options.TenantIdClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(externalTenantId))
        {
            return ProductionTenantResolutionResult.Unresolved("tenant_claim_missing");
        }

        if (!Guid.TryParse(externalTenantId, out var tenantId) || tenantId == Guid.Empty)
        {
            return ProductionTenantResolutionResult.Unresolved("tenant_claim_invalid");
        }

        var subjectId = principal.FindFirst(_options.SubjectClaimType)?.Value;

        return ProductionTenantResolutionResult.Resolved(
            tenantId,
            externalTenantId,
            string.IsNullOrWhiteSpace(subjectId) ? null : subjectId);
    }
}
