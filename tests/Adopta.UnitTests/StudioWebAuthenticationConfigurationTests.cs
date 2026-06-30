using Adopta.Web.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adopta.UnitTests;

public sealed class StudioWebAuthenticationConfigurationTests
{
    [Fact]
    public void Disabled_web_authentication_configuration_is_valid_by_default()
    {
        var configuration = new ConfigurationBuilder().Build();

        var validation = StudioWebAuthenticationConfigurationValidator.Validate(configuration);
        var authenticationOptions =
            StudioWebAuthenticationConfigurationValidator.ReadAuthenticationOptions(configuration);
        var tokenOptions =
            StudioWebAuthenticationConfigurationValidator.ReadTokenAcquisitionOptions(configuration);

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Issues);
        Assert.False(authenticationOptions.Enabled);
        Assert.False(tokenOptions.Enabled);
        Assert.Empty(tokenOptions.Scopes);
    }

    [Fact]
    public void Enabled_web_authentication_requires_complete_safe_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:StudioWeb:Enabled"] = "true",
                ["Authentication:StudioWeb:Authority"] = CreateInsecureAuthority(),
                ["Authentication:StudioWeb:ClientId"] = string.Empty,
                ["Authentication:StudioWeb:CallbackPath"] = "signin-oidc"
            })
            .Build();

        var validation = StudioWebAuthenticationConfigurationValidator.Validate(configuration);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Issues, issue => issue.Code == "studio_web_authentication_incomplete");
        AssertSafeIssues(validation.Issues);
    }

    [Fact]
    public void Enabled_token_acquisition_requires_web_authentication_and_scopes()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["StudioApi:TokenAcquisition:Enabled"] = "true"
            })
            .Build();

        var validation = StudioWebAuthenticationConfigurationValidator.Validate(configuration);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Issues, issue => issue.Code == "studio_api_token_requires_web_authentication");
        Assert.Contains(validation.Issues, issue => issue.Code == "studio_api_scopes_missing");
        AssertSafeIssues(validation.Issues);
    }

    [Fact]
    public void Complete_explicit_configuration_can_enable_token_provider_registration()
    {
        var configuration = BuildEnabledConfiguration();
        var services = new ServiceCollection();

        services.AddStudioApiBoundary(configuration);
        services.AddStudioWebAuthenticationSeam(configuration);

        using var provider = services.BuildServiceProvider();
        var authenticationOptions = provider.GetRequiredService<IOptions<StudioWebAuthenticationOptions>>().Value;
        var tokenOptions = provider.GetRequiredService<IOptions<StudioApiTokenAcquisitionOptions>>().Value;
        var tokenProvider = provider.GetRequiredService<IStudioApiAccessTokenProvider>();

        Assert.True(authenticationOptions.Enabled);
        Assert.True(authenticationOptions.HasCompleteConfiguration);
        Assert.True(tokenOptions.Enabled);
        Assert.True(tokenOptions.HasConfiguredScopes);
        Assert.IsType<MicrosoftIdentityStudioApiAccessTokenProvider>(tokenProvider);
    }

    [Fact]
    public void Invalid_configuration_fails_with_generic_non_sensitive_exception()
    {
        var invalidClientId = CreateInvalidClientId();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:StudioWeb:Enabled"] = "true",
                ["Authentication:StudioWeb:Authority"] = CreateSafeHttpsAuthority(),
                ["Authentication:StudioWeb:ClientId"] = invalidClientId
            })
            .Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddStudioWebAuthenticationSeam(configuration));

        Assert.Equal(StudioWebAuthenticationConfigurationValidator.SafeConfigurationErrorMessage, exception.Message);
        AssertSafeMessage(exception.Message);
        Assert.DoesNotContain(invalidClientId, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Program_registers_web_authentication_seam_without_live_studio_activation()
    {
        var program = ReadRepositoryFile("src/Adopta.Web/Program.cs");

        Assert.Contains("builder.Services.AddStudioApiBoundary(builder.Configuration);", program, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddStudioWebAuthenticationSeam(builder.Configuration);", program, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddScoped<IStudioContentClient, LocalStudioContentClient>();", program, StringComparison.Ordinal);
        Assert.DoesNotContain("StudioAuthoringReadApiClient", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHttpClient<IStudioContentClient", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Appsettings_are_not_changed_to_hold_auth_or_api_values()
    {
        var appsettings = ReadRepositoryFile("src/Adopta.Web/appsettings.json");
        var development = ReadRepositoryFile("src/Adopta.Web/appsettings.Development.json");
        var combined = string.Concat(appsettings, development);

        Assert.DoesNotContain("StudioWeb", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("TokenAcquisition", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ClientSecret", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password=", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer ", combined, StringComparison.OrdinalIgnoreCase);
    }

    private static IConfiguration BuildEnabledConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:StudioWeb:Enabled"] = "true",
                ["Authentication:StudioWeb:Authority"] = CreateSafeHttpsAuthority(),
                ["Authentication:StudioWeb:ClientId"] = Guid.NewGuid().ToString(),
                ["Authentication:StudioWeb:CallbackPath"] = "/signin-oidc",
                ["StudioApi:TokenAcquisition:Enabled"] = "true",
                ["StudioApi:TokenAcquisition:Scopes:0"] = CreateSafeScope()
            })
            .Build();
    }

    private static string CreateSafeHttpsAuthority() =>
        new UriBuilder(Uri.UriSchemeHttps, "localhost").Uri.AbsoluteUri;

    private static string CreateInsecureAuthority() =>
        new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri.AbsoluteUri;

    private static string CreateSafeScope() =>
        $"api://{Guid.NewGuid():D}/access";

    private static string CreateInvalidClientId() =>
        $"invalid-{Guid.NewGuid():N}";

    private static void AssertSafeIssues(IEnumerable<StudioWebAuthenticationValidationIssue> issues)
    {
        Assert.All(issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Code));
            AssertSafeMessage(issue.Message);
        });
    }

    private static void AssertSafeMessage(string message)
    {
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.DoesNotContain("token", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("client", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("authority", message, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Adopta.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return File.ReadAllText(Path.Combine(directory.FullName, relativePath));
    }
}
