using System.Security.Claims;
using Adopta.Application.Abstractions;
using Adopta.Application.Identity;
using Microsoft.Extensions.Options;

namespace Adopta.Infrastructure.Identity;

public sealed class ClaimsPrincipalProductionTenantResolver : IProductionTenantResolver
{
    private readonly IAdoptaTenantMappingService _tenantMappingService;
    private readonly EntraTenantResolutionOptions _options;

    public ClaimsPrincipalProductionTenantResolver(
        IAdoptaTenantMappingService tenantMappingService,
        IOptions<EntraTenantResolutionOptions> options)
    {
        _tenantMappingService = tenantMappingService;
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

        if (!Guid.TryParse(externalTenantId, out var parsedExternalTenantId) || parsedExternalTenantId == Guid.Empty)
        {
            return ProductionTenantResolutionResult.Unresolved("tenant_claim_invalid");
        }

        var applicationId = principal.FindFirst(_options.ApplicationIdClaimType)?.Value
            ?? principal.FindFirst(_options.FallbackApplicationIdClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            return ProductionTenantResolutionResult.Unresolved("application_claim_missing");
        }

        var tenantMapping = _tenantMappingService.MapTenant(externalTenantId, applicationId);
        if (!tenantMapping.IsMapped || !tenantMapping.TenantId.HasValue)
        {
            return ProductionTenantResolutionResult.Unresolved(tenantMapping.FailureCode);
        }

        var subjectId = principal.FindFirst(_options.SubjectClaimType)?.Value;

        return ProductionTenantResolutionResult.Resolved(
            tenantMapping.TenantId.Value,
            externalTenantId,
            string.IsNullOrWhiteSpace(subjectId) ? null : subjectId);
    }
}
