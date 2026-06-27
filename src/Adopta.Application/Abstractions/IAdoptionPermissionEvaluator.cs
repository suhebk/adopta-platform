using Adopta.Domain.Identity;

namespace Adopta.Application.Abstractions;

public interface IAdoptionPermissionEvaluator
{
    bool IsAllowed(AdoptionUser? user, string permissionKey);
}
