using System.Security.Claims;
using Adopta.Application.Identity;

namespace Adopta.Application.Abstractions;

public interface IAuthenticatedUserMappingService
{
    AuthenticatedUserMappingResult MapUser(Guid tenantId, ClaimsPrincipal principal);
}
