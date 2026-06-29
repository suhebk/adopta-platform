using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Persistence;

namespace Adopta.IntegrationTests;

public sealed class EfAuthoredContentRepositoryTenantIsolationTests
{
    [Fact]
    public async Task Tenant_can_read_and_list_own_authored_content()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantId = Guid.NewGuid();
        var repository = CreateRepository(databaseName, tenantId);
        var content = CreateContent(tenantId);

        await repository.AddAsync(content);

        var found = await repository.GetByIdAsync(tenantId, content.Id);
        var listed = await repository.ListAsync(tenantId);

        Assert.NotNull(found);
        Assert.Equal(content.Id, found.Id);
        Assert.Single(listed);
    }

    [Fact]
    public async Task Tenant_cannot_read_or_list_other_tenant_authored_content()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateRepository(databaseName, tenantA);
        var repositoryB = CreateRepository(databaseName, tenantB);
        var contentB = CreateContent(tenantB);

        await repositoryB.AddAsync(contentB);

        var hidden = await repositoryA.GetByIdAsync(tenantA, contentB.Id);
        var listed = await repositoryA.ListAsync(tenantA);
        var denied = await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.GetByIdAsync(tenantB, contentB.Id));

        Assert.Null(hidden);
        Assert.Empty(listed);
        Assert.Equal("Tenant access denied.", denied.Message);
    }

    [Fact]
    public async Task Tenant_cannot_write_other_tenant_authored_content()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repositoryA = CreateRepository(databaseName, tenantA);

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repositoryA.AddAsync(CreateContent(tenantB)));

        Assert.Equal("Tenant access denied.", ex.Message);
        Assert.DoesNotContain(tenantB.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Missing_tenant_context_fails_closed()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        using var dbContext = EfRepositoryTestFactory.CreateDbContext(databaseName);
        var repository = new EfAuthoredContentRepository(
            dbContext,
            EfRepositoryTestFactory.CreateTenantContext());

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.ListAsync(Guid.NewGuid()));
    }

    private static EfAuthoredContentRepository CreateRepository(string databaseName, Guid tenantId)
    {
        return new EfAuthoredContentRepository(
            EfRepositoryTestFactory.CreateDbContext(databaseName),
            EfRepositoryTestFactory.CreateTenantContext(tenantId));
    }

    private static AuthoredContentItem CreateContent(Guid tenantId)
    {
        return new AuthoredContentItem(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            $"content.{Guid.NewGuid():N}",
            "Tenant scoped content",
            [
                new AuthoredContentVersion(
                    Guid.NewGuid(),
                    "1.0.0",
                    ContentLifecycleState.Draft,
                    DateTimeOffset.UtcNow)
            ]);
    }
}
