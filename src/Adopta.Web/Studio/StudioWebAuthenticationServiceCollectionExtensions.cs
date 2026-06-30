using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Adopta.Web.Studio;

public static class StudioWebAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddStudioWebAuthenticationSeam(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authenticationOptions =
            StudioWebAuthenticationConfigurationValidator.ReadAuthenticationOptions(configuration);
        var tokenAcquisitionOptions =
            StudioWebAuthenticationConfigurationValidator.ReadTokenAcquisitionOptions(configuration);
        var validation = StudioWebAuthenticationConfigurationValidator.Validate(
            authenticationOptions,
            tokenAcquisitionOptions);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                StudioWebAuthenticationConfigurationValidator.SafeConfigurationErrorMessage);
        }

        services.Configure<StudioWebAuthenticationOptions>(options =>
        {
            options.Enabled = authenticationOptions.Enabled;
            options.Authority = authenticationOptions.Authority;
            options.ClientId = authenticationOptions.ClientId;
            options.CallbackPath = authenticationOptions.CallbackPath;
        });
        services.Configure<StudioApiTokenAcquisitionOptions>(options =>
        {
            options.Enabled = tokenAcquisitionOptions.Enabled;
            options.Scopes = tokenAcquisitionOptions.Scopes;
        });

        services.AddHttpContextAccessor();

        if (authenticationOptions.Enabled && tokenAcquisitionOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IStudioApiAccessTokenProvider, MicrosoftIdentityStudioApiAccessTokenProvider>());
        }

        return services;
    }
}
