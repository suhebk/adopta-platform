using Adopta.Domain.Identity;

namespace Adopta.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> ListAsync(CancellationToken cancellationToken = default);
}
