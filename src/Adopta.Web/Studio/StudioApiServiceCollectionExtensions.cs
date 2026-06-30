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

    public static IServiceCollection AddStudioReadApiActivationGate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiOptions = StudioReadApiActivationValidator.ReadApiOptions(configuration);
        var authenticationOptions =
            StudioWebAuthenticationConfigurationValidator.ReadAuthenticationOptions(configuration);
        var tokenAcquisitionOptions =
            StudioWebAuthenticationConfigurationValidator.ReadTokenAcquisitionOptions(configuration);
        var authenticationValidation = StudioWebAuthenticationConfigurationValidator.Validate(
            authenticationOptions,
            tokenAcquisitionOptions);
        if (authenticationValidation.IsValid)
        {
            services.AddStudioWebAuthenticationSeam(configuration);
        }

        var activation = StudioReadApiActivationValidator.Validate(
            apiOptions,
            authenticationOptions,
            tokenAcquisitionOptions);
        if (!activation.CanActivate)
        {
            services.AddScoped<IStudioContentClient, LocalStudioContentClient>();
            return services;
        }

        services.AddHttpClient<StudioAuthoringReadApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiOptions.BaseAddress, UriKind.Absolute);
        })
            .AddHttpMessageHandler<StudioApiRequestBoundaryHandler>();
        services.AddScoped<IStudioContentClient>(serviceProvider =>
            serviceProvider.GetRequiredService<StudioAuthoringReadApiClient>());

        return services;
    }
}
