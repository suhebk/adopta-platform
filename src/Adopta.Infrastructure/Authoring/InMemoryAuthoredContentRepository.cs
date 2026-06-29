using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Persistence;

namespace Adopta.Infrastructure.Authoring;

public sealed class InMemoryAuthoredContentRepository : IAuthoredContentRepository
{
    private static readonly List<AuthoredContentItem> Items = [];
    private static readonly object Gate = new();
    private readonly IAdoptionTenantContext _tenantContext;

    public InMemoryAuthoredContentRepository(IAdoptionTenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Task AddAsync(
        AuthoredContentItem content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, content.TenantId);

        lock (Gate)
        {
            Items.RemoveAll(existing => existing.TenantId == content.TenantId && existing.Id == content.Id);
            Items.Add(content);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        AuthoredContentItem content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, content.TenantId);

        lock (Gate)
        {
            var index = Items.FindIndex(existing => existing.TenantId == content.TenantId && existing.Id == content.Id);
            if (index >= 0)
            {
                Items[index] = content;
            }
            else
            {
                Items.Add(content);
            }
        }

        return Task.CompletedTask;
    }

    public Task<AuthoredContentItem?> GetByIdAsync(
        Guid tenantId,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, tenantId);

        lock (Gate)
        {
            return Task.FromResult(Items.SingleOrDefault(item =>
                item.TenantId == tenantId && item.Id == contentId));
        }
    }

    public Task<IReadOnlyCollection<AuthoredContentItem>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        TenantRepositoryGuard.RequireTenantMatch(_tenantContext, tenantId);

        lock (Gate)
        {
            return Task.FromResult<IReadOnlyCollection<AuthoredContentItem>>(
                Items.Where(item => item.TenantId == tenantId).ToArray());
        }
    }
}
