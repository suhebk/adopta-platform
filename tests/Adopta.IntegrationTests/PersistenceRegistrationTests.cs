using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Infrastructure;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.IntegrationTests;

public sealed class PersistenceRegistrationTests
{
    [Fact]
    public void Default_configuration_uses_in_memory_repositories()
    {
        var services = new ServiceCollection();

        services.AddAdoptaInfrastructure(new ConfigurationBuilder().Build());

        AssertRegistration<IAuthoredContentRepository, InMemoryAuthoredContentRepository>(services);
        AssertRegistration<ITenantMappingRepository, InMemoryTenantMappingRepository>(services);
        AssertRegistration<IAuthenticatedUserMappingRepository, InMemoryAuthenticatedUserMappingRepository>(services);
        AssertRegistration<IAuditEventRepository, InMemoryAuditEventRepository>(services);
        AssertRegistration<ISecurityAuditEventRepository, InMemorySecurityAuditEventRepository>(services);
        AssertRegistration<IAuthoredContentLifecycleHistoryRepository, InMemoryAuthoredContentLifecycleHistoryRepository>(
            services);
        AssertRegistration<IAuthoredContentPublishingHistoryRepository, InMemoryAuthoredContentPublishingHistoryRepository>(
            services);
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(AdoptaDbContext));
    }

    [Fact]
    public void Sql_server_persistence_registers_only_approved_ef_repositories()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Enabled"] = "true",
                ["Persistence:Provider"] = "SqlServer",
                ["Persistence:SqlServer:ConnectionStringName"] = "AdoptaSqlServer",
                ["ConnectionStrings:AdoptaSqlServer"] = "__configured_by_secure_provider__"
            })
            .Build();

        services.AddAdoptaInfrastructure(configuration);

        AssertRegistration<IAuthoredContentRepository, EfAuthoredContentRepository>(services);
        AssertRegistration<ITenantMappingRepository, EfTenantMappingRepository>(services);
        AssertRegistration<IAuthenticatedUserMappingRepository, EfAuthenticatedUserMappingRepository>(services);
        AssertRegistration<IAuditEventRepository, EfAuditEventRepository>(services);
        AssertRegistration<ISecurityAuditEventRepository, EfSecurityAuditEventRepository>(services);
        AssertRegistration<IAuthoredContentLifecycleHistoryRepository, EfAuthoredContentLifecycleHistoryRepository>(
            services);
        AssertRegistration<IAuthoredContentPublishingHistoryRepository, EfAuthoredContentPublishingHistoryRepository>(
            services);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(AdoptaDbContext));
        AssertRegistration<ITenantApplicationRepository, InMemoryTenantApplicationRepository>(services);
        AssertRegistration<IAdoptionUserRepository, InMemoryAdoptionUserRepository>(services);
        AssertRegistration<IRoleRepository, InMemoryRoleRepository>(services);
    }

    private static void AssertRegistration<TService, TImplementation>(IServiceCollection services)
    {
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(TService)
            && descriptor.ImplementationType == typeof(TImplementation));
    }
}
