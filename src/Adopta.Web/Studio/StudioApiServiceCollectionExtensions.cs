using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Adopta.Web.Studio;

public static class StudioApiServiceCollectionExtensions
{
    public static IServiceCollection AddStudioApiBoundary(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StudioApiClientOptions>(
            configuration.GetSection(StudioApiClientOptions.SectionName));
        services.TryAddScoped<IStudioApiAccessTokenProvider, UnavailableStudioApiAccessTokenProvider>();
        services.AddTransient<StudioApiRequestBoundaryHandler>();

        return services;
    }
}
