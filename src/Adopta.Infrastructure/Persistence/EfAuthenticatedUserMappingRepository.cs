using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Identity;
using Adopta.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Persistence;

public sealed class EfAuthenticatedUserMappingRepository : IAuthenticatedUserMappingRepository
{
    private readonly AdoptaDbContext _dbContext;
    private readonly IAdoptionTenantContext _tenantContext;

    public EfAuthenticatedUserMappingRepository(
        AdoptaDbContext dbContext,
        IAdoptionTenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(
        AuthenticatedUserMappingRecord mapping,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenantMatch(_tenantContext, mapping.TenantId);
        if (mapping.User.TenantId != tenantId)
        {
            throw new TenantAccessDeniedException();
        }

        var userExists = await _dbContext.AdoptionUsers
            .AsNoTracking()
            .AnyAsync(user => user.TenantId == tenantId && user.Id == mapping.User.Id, cancellationToken);
        if (!userExists)
        {
            throw new TenantAccessDeniedException();
        }

        _dbContext.AuthenticatedUserMappings.Add(new AuthenticatedUserMappingEntity(
            mapping.TenantId,
            mapping.ExternalSubjectId,
            mapping.User.Id));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthenticatedUserMappingRecord?> FindAsync(
        string externalSubjectId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantRepositoryGuard.RequireTenant(_tenantContext);

        var result = await _dbContext.AuthenticatedUserMappings
            .AsNoTracking()
            .Where(mapping => mapping.TenantId == tenantId && mapping.ExternalSubjectId == externalSubjectId)
            .Join(
                _dbContext.AdoptionUsers.AsNoTracking(),
                mapping => new { mapping.TenantId, Id = mapping.UserId },
                user => new { user.TenantId, user.Id },
                (mapping, user) => new MappingProjection(mapping.TenantId, mapping.ExternalSubjectId, user))
            .SingleOrDefaultAsync(cancellationToken);

        return result is null
            ? null
            : new AuthenticatedUserMappingRecord(result.TenantId, result.ExternalSubjectId, result.User);
    }

    private sealed record MappingProjection(
        Guid TenantId,
        string ExternalSubjectId,
        AdoptionUser User);
}
