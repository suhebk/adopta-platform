using Adopta.Application.Identity;

namespace Adopta.Application.Abstractions.Persistence;

public interface IAuthenticatedUserMappingRepository
{
    Task AddAsync(AuthenticatedUserMappingRecord mapping, CancellationToken cancellationToken = default);

    Task<AuthenticatedUserMappingRecord?> FindAsync(
        string externalSubjectId,
        CancellationToken cancellationToken = default);
}
