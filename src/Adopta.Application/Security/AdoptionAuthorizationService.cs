using Adopta.Application.Abstractions;
using Adopta.Domain.Identity;

namespace Adopta.Application.Security;

public sealed class AdoptionAuthorizationService : IAdoptionAuthorizationService
{
    public bool HasPermission(AdoptionUser user, string permissionKey)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(permissionKey))
        {
            return false;
        }

        return user.Roles
            .SelectMany(role => role.Permissions)
            .Any(permission => permission.Key.Equals(permissionKey, StringComparison.Ordinal));
    }
}
