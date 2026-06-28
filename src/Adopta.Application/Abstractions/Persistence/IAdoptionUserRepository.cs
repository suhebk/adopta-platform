using Adopta.Domain.Identity;

namespace Adopta.Application.Abstractions.Persistence;

public interface IAdoptionUserRepository
{
    Task AddAsync(AdoptionUser user, CancellationToken cancellationToken = default);

    Task<AdoptionUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AdoptionUser>> ListAsync(CancellationToken cancellationToken = default);
}
