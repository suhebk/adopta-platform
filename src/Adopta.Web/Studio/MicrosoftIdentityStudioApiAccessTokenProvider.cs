using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Adopta.Web.Studio;

public sealed class MicrosoftIdentityStudioApiAccessTokenProvider : IStudioApiAccessTokenProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IOptions<StudioWebAuthenticationOptions> authenticationOptions;
    private readonly IOptions<StudioApiTokenAcquisitionOptions> tokenAcquisitionOptions;

    public MicrosoftIdentityStudioApiAccessTokenProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<StudioWebAuthenticationOptions> authenticationOptions,
        IOptions<StudioApiTokenAcquisitionOptions> tokenAcquisitionOptions)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.authenticationOptions = authenticationOptions;
        this.tokenAcquisitionOptions = tokenAcquisitionOptions;
    }

    public async Task<StudioApiAccessTokenResult> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validation = StudioWebAuthenticationConfigurationValidator.Validate(
            authenticationOptions.Value,
            tokenAcquisitionOptions.Value);
        if (!validation.IsValid
            || !authenticationOptions.Value.Enabled
            || !tokenAcquisitionOptions.Value.Enabled)
        {
            return StudioApiAccessTokenResult.Unavailable();
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
        {
            return StudioApiAccessTokenResult.Unavailable();
        }

        try
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");

            return string.IsNullOrWhiteSpace(accessToken)
                ? StudioApiAccessTokenResult.Unavailable()
                : StudioApiAccessTokenResult.Available(accessToken);
        }
        catch
        {
            return StudioApiAccessTokenResult.Unavailable();
        }
    }
}
