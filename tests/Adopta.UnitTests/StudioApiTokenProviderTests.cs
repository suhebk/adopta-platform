using System.Security.Claims;
using Adopta.Web.Studio;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adopta.UnitTests;

public sealed class StudioApiTokenProviderTests
{
    [Fact]
    public async Task Provider_returns_unavailable_when_disabled()
    {
        var provider = BuildProvider(
            new StudioWebAuthenticationOptions(),
            new StudioApiTokenAcquisitionOptions(),
            CreateAuthenticatedContext(CreateSafeAccessValue()));

        var result = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(StudioApiAccessTokenStatus.Unavailable, result.Status);
        Assert.Null(result.AccessToken);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Provider_returns_unavailable_when_http_context_is_missing()
    {
        var provider = BuildProvider(
            BuildEnabledAuthenticationOptions(),
            BuildEnabledTokenOptions(),
            httpContext: null);

        var result = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(StudioApiAccessTokenStatus.Unavailable, result.Status);
        Assert.Null(result.AccessToken);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Provider_returns_unavailable_when_user_is_not_authenticated()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };
        var provider = BuildProvider(
            BuildEnabledAuthenticationOptions(),
            BuildEnabledTokenOptions(),
            context);

        var result = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(StudioApiAccessTokenStatus.Unavailable, result.Status);
        Assert.Null(result.AccessToken);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Provider_returns_server_side_access_token_without_exposing_it_in_message()
    {
        var accessValue = CreateSafeAccessValue();
        var provider = BuildProvider(
            BuildEnabledAuthenticationOptions(),
            BuildEnabledTokenOptions(),
            CreateAuthenticatedContext(accessValue));

        var result = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(StudioApiAccessTokenStatus.Available, result.Status);
        Assert.Equal(accessValue, result.AccessToken);
        Assert.DoesNotContain(accessValue, result.SafeMessage, StringComparison.Ordinal);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Provider_token_acquisition_failure_does_not_expose_raw_exception()
    {
        var context = CreateAuthenticatedContext("unused");
        context.RequestServices = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(
                new ThrowingAuthenticationService("Bearer token ConnectionString=unsafe"))
            .BuildServiceProvider();
        var provider = BuildProvider(
            BuildEnabledAuthenticationOptions(),
            BuildEnabledTokenOptions(),
            context);

        var result = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(StudioApiAccessTokenStatus.Unavailable, result.Status);
        Assert.Null(result.AccessToken);
        AssertSafeMessage(result.SafeMessage);
        Assert.DoesNotContain("Bearer", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Provider_constructor_shape_does_not_accept_tenant_id_or_browser_token()
    {
        var constructors = typeof(MicrosoftIdentityStudioApiAccessTokenProvider).GetConstructors();
        var parameterNames = constructors
            .SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(parameterNames, name => name.Contains("tenant", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(parameterNames, name => name.Contains("browser", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(parameterNames, name => name.Contains("accessToken", StringComparison.OrdinalIgnoreCase));
    }

    private static MicrosoftIdentityStudioApiAccessTokenProvider BuildProvider(
        StudioWebAuthenticationOptions authenticationOptions,
        StudioApiTokenAcquisitionOptions tokenOptions,
        HttpContext? httpContext)
    {
        return new MicrosoftIdentityStudioApiAccessTokenProvider(
            new HttpContextAccessor
            {
                HttpContext = httpContext
            },
            Options.Create(authenticationOptions),
            Options.Create(tokenOptions));
    }

    private static StudioWebAuthenticationOptions BuildEnabledAuthenticationOptions() =>
        new()
        {
            Enabled = true,
            Authority = CreateSafeHttpsAuthority(),
            ClientId = Guid.NewGuid().ToString(),
            CallbackPath = "/signin-oidc"
        };

    private static StudioApiTokenAcquisitionOptions BuildEnabledTokenOptions() =>
        new()
        {
            Enabled = true,
            Scopes = [CreateSafeScope()]
        };

    private static string CreateSafeHttpsAuthority() =>
        new UriBuilder(Uri.UriSchemeHttps, "localhost").Uri.AbsoluteUri;

    private static string CreateSafeScope() =>
        $"api://{Guid.NewGuid():D}/access";

    private static string CreateSafeAccessValue() =>
        Guid.NewGuid().ToString("N");

    private static DefaultHttpContext CreateAuthenticatedContext(string accessValue)
    {
        var authenticationProperties = new AuthenticationProperties();
        authenticationProperties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = "access_token",
                Value = accessValue
            }
        ]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", "server-user")],
                authenticationType: "Test"))
        };
        context.RequestServices = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(
                new StaticAuthenticationService(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        context.User,
                        authenticationProperties,
                        "Test"))))
            .BuildServiceProvider();

        return context;
    }

    private static void AssertSafeMessage(string message)
    {
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.DoesNotContain("token", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("authorization", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StaticAuthenticationService : IAuthenticationService
    {
        private readonly AuthenticateResult result;

        public StaticAuthenticationService(AuthenticateResult result)
        {
            this.result = result;
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
            Task.FromResult(result);

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignInAsync(
            HttpContext context,
            string? scheme,
            ClaimsPrincipal principal,
            AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;
    }

    private sealed class ThrowingAuthenticationService : IAuthenticationService
    {
        private readonly string message;

        public ThrowingAuthenticationService(string message)
        {
            this.message = message;
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        {
            throw new InvalidOperationException(message);
        }

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignInAsync(
            HttpContext context,
            string? scheme,
            ClaimsPrincipal principal,
            AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;
    }
}
