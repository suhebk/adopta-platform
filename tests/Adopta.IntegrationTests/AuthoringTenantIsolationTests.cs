using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class AuthoringTenantIsolationTests
{
    [Fact]
    public async Task Tenant_can_access_own_authored_content()
    {
        var tenantId = Guid.NewGuid();
        var repository = BuildRepository(tenantId);
        var content = BuildContent(tenantId);

        await repository.AddAsync(content);

        var stored = await repository.GetByIdAsync(tenantId, content.Id);
        var listed = await repository.ListAsync(tenantId);

        Assert.Same(content, stored);
        Assert.Contains(listed, item => item.Id == content.Id && item.TenantId == tenantId);
    }

    [Fact]
    public async Task Tenant_cannot_access_other_tenant_authored_content()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tenantARepository = BuildRepository(tenantA);
        var tenantBRepository = BuildRepository(tenantB);
        var tenantBContent = BuildContent(tenantB);

        await tenantBRepository.AddAsync(tenantBContent);

        var hidden = await tenantARepository.GetByIdAsync(tenantA, tenantBContent.Id);
        var tenantAItems = await tenantARepository.ListAsync(tenantA);

        Assert.Null(hidden);
        Assert.DoesNotContain(tenantAItems, item => item.Id == tenantBContent.Id);
        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            tenantARepository.GetByIdAsync(tenantB, tenantBContent.Id));
    }

    [Fact]
    public async Task Missing_tenant_context_is_denied()
    {
        var repository = BuildRepository();

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.ListAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Cross_tenant_write_is_denied_without_leaking_tenant_details()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repository = BuildRepository(tenantA);
        var tenantBContent = BuildContent(tenantB);

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() =>
            repository.AddAsync(tenantBContent));

        Assert.Equal("Tenant access denied.", ex.Message);
        Assert.DoesNotContain(tenantB.ToString(), ex.Message, StringComparison.Ordinal);
    }

    private static IAuthoredContentRepository BuildRepository(Guid? tenantId = null)
    {
        var services = new ServiceCollection();
        var context = new AdoptionTenantContext();
        if (tenantId.HasValue)
        {
            context.SetTenant(tenantId.Value);
        }

        services.AddScoped<AdoptionTenantContext>(_ => context);
        services.AddScoped<IAdoptionTenantContext>(_ => context);
        services.AddScoped<IAuthoredContentRepository, InMemoryAuthoredContentRepository>();

        return services.BuildServiceProvider().GetRequiredService<IAuthoredContentRepository>();
    }

    private static AuthoredContentItem BuildContent(Guid tenantId)
    {
        return new AuthoredContentItem(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            AuthoredContentType.Tooltip,
            "billing.submit",
            "Submit return",
            [
                new AuthoredContentVersion(
                    Guid.NewGuid(),
                    "1.0.0",
                    ContentLifecycleState.Draft,
                    DateTimeOffset.UtcNow)
            ]);
    }
}
