using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Adopta.Api.Auth;

public static class AuthenticationServiceCollectionExtensions
{
    public const string TestAuthenticationScheme = "Adopta.Test";

    public static IServiceCollection AddAdoptaAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AdoptaAuthenticationOptions>(
            configuration.GetSection(AdoptaAuthenticationOptions.SectionName));

        var testAuthenticationEnabled = bool.TryParse(
            configuration["Authentication:Test:Enabled"],
            out var enabled)
            && enabled;
        var defaultScheme = testAuthenticationEnabled
            ? TestAuthenticationScheme
            : JwtBearerDefaults.AuthenticationScheme;

        var authenticationBuilder = services
            .AddAuthentication(defaultScheme);

        authenticationBuilder.AddJwtBearer(options =>
        {
            var authSection = configuration.GetSection(AdoptaAuthenticationOptions.SectionName);
            var authority = authSection[nameof(AdoptaAuthenticationOptions.Authority)];
            var audience = authSection[nameof(AdoptaAuthenticationOptions.Audience)];

            options.MapInboundClaims = false;
            options.RequireHttpsMetadata = true;

            if (!string.IsNullOrWhiteSpace(authority))
            {
                options.Authority = authority;
            }

            if (!string.IsNullOrWhiteSpace(audience))
            {
                options.Audience = audience;
            }
        });

        if (testAuthenticationEnabled)
        {
            authenticationBuilder.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationScheme,
                _ => { });
        }

        services.AddAuthorization();

        return services;
    }
}

public sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticatedHeaderName = "X-Adopta-Test-Authenticated";
    public const string TenantHeaderName = "X-Adopta-Test-Tid";
    public const string ApplicationHeaderName = "X-Adopta-Test-AppId";
    public const string SubjectHeaderName = "X-Adopta-Test-Oid";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthenticatedHeaderName, out var authenticated)
            || !bool.TryParse(authenticated.ToString(), out var isAuthenticated)
            || !isAuthenticated)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>();

        if (Request.Headers.TryGetValue(TenantHeaderName, out var tenantId))
        {
            claims.Add(new Claim("tid", tenantId.ToString()));
        }

        if (Request.Headers.TryGetValue(ApplicationHeaderName, out var applicationId))
        {
            claims.Add(new Claim("azp", applicationId.ToString()));
        }

        if (Request.Headers.TryGetValue(SubjectHeaderName, out var subjectId))
        {
            claims.Add(new Claim("oid", subjectId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
