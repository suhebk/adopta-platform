using Adopta.Domain.Identity;

namespace Adopta.Application.Abstractions;

public interface IAdoptionAuthorizationService
{
    bool HasPermission(AdoptionUser user, string permissionKey);
}
