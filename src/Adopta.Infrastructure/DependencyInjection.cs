using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Abstractions.Runtime;
using Adopta.Domain.Tenancy;
using Adopta.Infrastructure.Audit;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Identity;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Runtime;
using Adopta.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdoptaInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        if (configuration is not null)
        {
            services.Configure<EntraTenantResolutionOptions>(options =>
            {
                var section = configuration.GetSection(EntraTenantResolutionOptions.SectionName);
                options.TenantIdClaimType = section[nameof(EntraTenantResolutionOptions.TenantIdClaimType)]
                    ?? options.TenantIdClaimType;
                options.ApplicationIdClaimType = section[nameof(EntraTenantResolutionOptions.ApplicationIdClaimType)]
                    ?? options.ApplicationIdClaimType;
                options.FallbackApplicationIdClaimType = section[nameof(EntraTenantResolutionOptions.FallbackApplicationIdClaimType)]
                    ?? options.FallbackApplicationIdClaimType;
                options.SubjectClaimType = section[nameof(EntraTenantResolutionOptions.SubjectClaimType)]
                    ?? options.SubjectClaimType;
            });
        }
        else
        {
            services.Configure<EntraTenantResolutionOptions>(_ => { });
        }

        var useEfPersistence = ConfigurePersistence(services, configuration);

        services.AddScoped<AdoptionTenantContext>();
        services.AddScoped<IAdoptionTenantContext>(serviceProvider =>
            serviceProvider.GetRequiredService<AdoptionTenantContext>());
        services.AddScoped<IProductionTenantResolver, ClaimsPrincipalProductionTenantResolver>();
        services.AddSingleton<InMemoryAdoptaTenantMappingStore>();
        services.AddSingleton<InMemoryAuthenticatedUserMappingStore>();
        services.AddScoped<IAdoptaTenantMappingService, InMemoryAdoptaTenantMappingService>();
        services.AddScoped<IAuthenticatedUserMappingService, InMemoryAuthenticatedUserMappingService>();

        services.AddSingleton<InMemoryAdoptionAuditStore>();
        services.AddSingleton<InMemoryAdoptionSecurityAuditStore>();
        services.AddSingleton<InMemoryTenantScopedStore<TenantApplication>>();

        services.AddScoped<IAdoptionAuditService, InMemoryAdoptionAuditService>();
        services.AddScoped<IAdoptionSecurityAuditService, InMemoryAdoptionSecurityAuditService>();
        services.AddScoped<IApplicationRegistrationService, InMemoryApplicationRegistrationService>();

        services.AddScoped<ITenantRepository, InMemoryTenantRepository>();
        services.AddScoped<ITenantApplicationRepository, InMemoryTenantApplicationRepository>();
        services.AddScoped<IAdoptionUserRepository, InMemoryAdoptionUserRepository>();
        services.AddScoped<IRoleRepository, InMemoryRoleRepository>();
        if (useEfPersistence)
        {
            services.AddScoped<ITenantMappingRepository, EfTenantMappingRepository>();
            services.AddScoped<IAuthenticatedUserMappingRepository, EfAuthenticatedUserMappingRepository>();
            services.AddScoped<ISecurityAuditEventRepository, EfSecurityAuditEventRepository>();
            services.AddScoped<IAuthoredContentRepository, EfAuthoredContentRepository>();
        }
        else
        {
            services.AddScoped<ITenantMappingRepository, InMemoryTenantMappingRepository>();
            services.AddScoped<IAuthenticatedUserMappingRepository, InMemoryAuthenticatedUserMappingRepository>();
            services.AddScoped<ISecurityAuditEventRepository, InMemorySecurityAuditEventRepository>();
            services.AddScoped<IAuthoredContentRepository, InMemoryAuthoredContentRepository>();
        }

        services.AddScoped<IAuditEventRepository, InMemoryAuditEventRepository>();
        services.AddScoped<IDeliveryBundleRepository, InMemoryDeliveryBundleRepository>();

        return services;
    }

    private static bool ConfigurePersistence(IServiceCollection services, IConfiguration? configuration)
    {
        if (configuration is null)
        {
            services.Configure<AdoptaPersistenceOptions>(_ => { });
            return false;
        }

        var section = configuration.GetSection(AdoptaPersistenceOptions.SectionName);
        services.Configure<AdoptaPersistenceOptions>(options =>
        {
            options.Enabled = bool.TryParse(section[nameof(AdoptaPersistenceOptions.Enabled)], out var enabled) && enabled;
            options.Provider = section[nameof(AdoptaPersistenceOptions.Provider)];
            options.SqlServer.ConnectionStringName =
                section.GetSection(nameof(AdoptaPersistenceOptions.SqlServer))[
                    nameof(SqlServerPersistenceOptions.ConnectionStringName)];
        });

        var options = new AdoptaPersistenceOptions
        {
            Enabled = bool.TryParse(section[nameof(AdoptaPersistenceOptions.Enabled)], out var enabled) && enabled,
            Provider = section[nameof(AdoptaPersistenceOptions.Provider)]
        };
        options.SqlServer.ConnectionStringName =
            section.GetSection(nameof(AdoptaPersistenceOptions.SqlServer))[
                nameof(SqlServerPersistenceOptions.ConnectionStringName)];

        if (!options.Enabled)
        {
            return false;
        }

        if (!string.Equals(options.Provider, "SqlServer", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Persistence is enabled but SQL Server persistence is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SqlServer.ConnectionStringName))
        {
            throw new InvalidOperationException("Persistence is enabled but SQL Server persistence is not configured.");
        }

        var connectionString = configuration.GetConnectionString(options.SqlServer.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Persistence is enabled but SQL Server persistence is not configured.");
        }

        services.AddDbContext<AdoptaDbContext>(dbContextOptions =>
            dbContextOptions.UseSqlServer(connectionString));
        return true;
    }
}
