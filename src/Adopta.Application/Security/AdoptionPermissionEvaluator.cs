using Adopta.Application.Abstractions;
using Adopta.Domain.Identity;

namespace Adopta.Application.Security;

public sealed class AdoptionPermissionEvaluator : IAdoptionPermissionEvaluator
{
    private readonly IAdoptionAuthorizationService _authorizationService;

    public AdoptionPermissionEvaluator(IAdoptionAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public bool IsAllowed(AdoptionUser? user, string permissionKey)
    {
        if (user is null || string.IsNullOrWhiteSpace(permissionKey))
        {
            return false;
        }

        try
        {
            return _authorizationService.HasPermission(user, permissionKey);
        }
        catch
        {
            return false;
        }
    }
}
