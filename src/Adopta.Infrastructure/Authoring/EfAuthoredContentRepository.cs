using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Authoring;

public sealed class EfAuthoredContentRepository : IAuthoredContentRepository
{
    private readonly AdoptaDbContext _dbContext;
    private readonly IAdoptionTenantContext _tenantContext;

    public EfAuthoredContentRepository(
        AdoptaDbContext dbContext,
        IAdoptionTenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(
        AuthoredContentItem content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        var tenantId = TenantRepositoryGuard.RequireTenantMatch(_tenantContext, content.TenantId);

        if (await HasCrossTenantContentIdAsync(content.Id, tenantId, cancellationToken))
        {
            throw new TenantAccessDeniedException();
        }

        _dbContext.AuthoredContentItems.Add(content);
        SetVersionTenantIds(content, tenantId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        AuthoredContentItem content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        var tenantId = TenantRepositoryGuard.RequireTenantMatch(_tenantContext, content.TenantId);

        if (await HasCrossTenantContentIdAsync(content.Id, tenantId, cancellationToken))
        {
            throw new TenantAccessDeniedException();
        }

        _dbContext.AuthoredContentItems.Update(content);
        SetVersionTenantIds(content, tenantId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthoredContentItem?> GetByIdAsync(
        Guid tenantId,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = TenantRepositoryGuard.RequireTenantMatch(_tenantContext, tenantId);

        return await _dbContext.AuthoredContentItems
            .AsNoTracking()
            .Include(content => content.Versions)
            .SingleOrDefaultAsync(
                content => content.TenantId == currentTenantId && content.Id == contentId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuthoredContentItem>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = TenantRepositoryGuard.RequireTenantMatch(_tenantContext, tenantId);

        return await _dbContext.AuthoredContentItems
            .AsNoTracking()
            .Include(content => content.Versions)
            .Where(content => content.TenantId == currentTenantId)
            .ToArrayAsync(cancellationToken);
    }

    private Task<bool> HasCrossTenantContentIdAsync(
        Guid contentId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return _dbContext.AuthoredContentItems
            .AsNoTracking()
            .AnyAsync(content => content.Id == contentId && content.TenantId != tenantId, cancellationToken);
    }

    private void SetVersionTenantIds(AuthoredContentItem content, Guid tenantId)
    {
        foreach (var version in content.Versions)
        {
            _dbContext.Entry(version).Property("TenantId").CurrentValue = tenantId;
        }
    }
}
