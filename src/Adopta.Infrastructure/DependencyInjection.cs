using Adopta.Application.Abstractions;
using Adopta.Domain.Tenancy;
using Adopta.Infrastructure.Audit;
using Adopta.Infrastructure.Identity;
using Adopta.Infrastructure.Tenancy;
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

        return services;
    }
}
