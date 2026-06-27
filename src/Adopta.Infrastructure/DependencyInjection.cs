using Adopta.Application.Abstractions;
using Adopta.Domain.Tenancy;
using Adopta.Infrastructure.Audit;
using Adopta.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdoptaInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<AdoptionTenantContext>();
        services.AddScoped<IAdoptionTenantContext>(serviceProvider =>
            serviceProvider.GetRequiredService<AdoptionTenantContext>());

        services.AddSingleton<InMemoryAdoptionAuditStore>();
        services.AddSingleton<InMemoryTenantScopedStore<TenantApplication>>();

        services.AddScoped<IAdoptionAuditService, InMemoryAdoptionAuditService>();
        services.AddScoped<IApplicationRegistrationService, InMemoryApplicationRegistrationService>();

        return services;
    }
}
