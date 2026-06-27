using System.Security.Claims;
using Adopta.Application.Abstractions;
using Adopta.Application.Identity;
using Microsoft.Extensions.Options;

namespace Adopta.Infrastructure.Identity;

public sealed class InMemoryAuthenticatedUserMappingService : IAuthenticatedUserMappingService
{
    private readonly InMemoryAuthenticatedUserMappingStore _store;
    private readonly EntraTenantResolutionOptions _options;

    public InMemoryAuthenticatedUserMappingService(
        InMemoryAuthenticatedUserMappingStore store,
        IOptions<EntraTenantResolutionOptions> options)
    {
        _store = store;
        _options = options.Value;
    }

    public AuthenticatedUserMappingResult MapUser(Guid tenantId, ClaimsPrincipal principal)
    {
        if (tenantId == Guid.Empty)
        {
            return AuthenticatedUserMappingResult.Unmapped("tenant_context_missing");
        }

        if (principal.Identity?.IsAuthenticated != true)
        {
            return AuthenticatedUserMappingResult.Unmapped("principal_not_authenticated");
        }

        var externalSubjectId = principal.FindFirst(_options.SubjectClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(externalSubjectId))
        {
            return AuthenticatedUserMappingResult.Unmapped("subject_claim_missing");
        }

        var matches = _store.Find(tenantId, externalSubjectId);

        return matches.Count switch
        {
            0 => AuthenticatedUserMappingResult.Unmapped("user_mapping_not_found"),
            1 => AuthenticatedUserMappingResult.Mapped(matches.Single().User),
            _ => AuthenticatedUserMappingResult.Unmapped("user_mapping_ambiguous")
        };
    }
}
