using Adopta.Domain.Authoring;

namespace Adopta.Application.Abstractions.Authoring;

public interface IAuthoredContentRepository
{
    Task AddAsync(
        AuthoredContentItem content,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        AuthoredContentItem content,
        CancellationToken cancellationToken = default);

    Task<AuthoredContentItem?> GetByIdAsync(
        Guid tenantId,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuthoredContentItem>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
