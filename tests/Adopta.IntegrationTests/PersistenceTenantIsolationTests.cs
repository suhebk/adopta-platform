using Adopta.Application.Abstractions.Persistence;
using Adopta.Domain.Tenancy;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class PersistenceTenantIsolationTests
{
    [Fact]
    public async Task Registered_repository_denies_cross_tenant_writes()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var services = BuildServices(tenantA);
        var repository = services.GetRequiredService<ITenantApplicationRepository>();
        var application = new TenantApplication(
            Guid.NewGuid(),
            tenantB,
            "Tenant B app",
            new Uri("https://tenant-b.example"));

        var ex = await Assert.ThrowsAsync<TenantAccessDeniedException>(() => repository.AddAsync(application));

        Assert.Equal("Tenant access denied.", ex.Message);
        Assert.DoesNotContain(tenantB.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Registered_repository_lists_only_current_tenant_records()
    {
        var tenantA = Guid.NewGuid();
        var services = BuildServices(tenantA);
        var repository = services.GetRequiredService<ITenantApplicationRepository>();
        var application = new TenantApplication(
            Guid.NewGuid(),
            tenantA,
            "Tenant A app",
            new Uri("https://tenant-a.example"));

        await repository.AddAsync(application);

        var applications = await repository.ListAsync();

        var stored = Assert.Single(applications);
        Assert.Equal(tenantA, stored.TenantId);
    }

    [Fact]
    public async Task Registered_repository_denies_missing_tenant_context()
    {
        var services = BuildServices();
        var repository = services.GetRequiredService<ITenantApplicationRepository>();

        await Assert.ThrowsAsync<TenantAccessDeniedException>(() => repository.ListAsync());
    }

    private static ServiceProvider BuildServices(Guid? tenantId = null)
    {
        var services = new ServiceCollection();
        var context = new AdoptionTenantContext();
        if (tenantId.HasValue)
        {
            context.SetTenant(tenantId.Value);
        }

        services.AddScoped(_ => context);
        services.AddScoped<Adopta.Application.Abstractions.IAdoptionTenantContext>(_ => context);
        services.AddScoped<ITenantApplicationRepository, InMemoryTenantApplicationRepository>();

        return services.BuildServiceProvider();
    }
}
