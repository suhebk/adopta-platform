using Adopta.Domain.Identity;

namespace Adopta.Application.Identity;

public sealed record AuthenticatedUserMappingResult(
    bool IsMapped,
    AdoptionUser? User,
    string FailureCode)
{
    public static AuthenticatedUserMappingResult Mapped(AdoptionUser user)
    {
        return new AuthenticatedUserMappingResult(true, user, string.Empty);
    }

    public static AuthenticatedUserMappingResult Unmapped(string failureCode)
    {
        return new AuthenticatedUserMappingResult(
            false,
            null,
            string.IsNullOrWhiteSpace(failureCode) ? "user_mapping_failed" : failureCode);
    }
}
