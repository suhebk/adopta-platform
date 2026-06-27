using Adopta.Application.Abstractions;
using Adopta.Application.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAdoptaApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdoptionAuthorizationService, AdoptionAuthorizationService>();

        return services;
    }
}
