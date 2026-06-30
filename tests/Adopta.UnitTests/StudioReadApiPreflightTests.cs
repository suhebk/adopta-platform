using Adopta.Application.Runtime;
using Adopta.Web.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.UnitTests;

public sealed class StudioReadApiPreflightTests
{
    private const string ReadinessGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ACTIVATION-READINESS.md";

    private const string Sprint10Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-10.md";

    [Fact]
    public async Task Default_configuration_remains_disabled_fail_closed_and_safe()
    {
        using var provider = BuildProvider(new ConfigurationBuilder().Build());

        var preflight = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);

        Assert.Equal(StudioReadApiPreflightStatus.Disabled, preflight.Status);
        Assert.False(preflight.IsReady);
        Assert.False(preflight.HasFailures);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioApiDisabledByDefault).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.LocalFallbackAvailable).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.ActivationFailsClosed).Status);
        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
        AssertSafePreflightOutput(preflight, []);
    }

    [Fact]
    public async Task Valid_explicit_configuration_passes_preflight_without_exposing_configured_values()
    {
        var values = BuildValidActivationValues();
        using var provider = BuildProvider(BuildConfiguration(values));

        var preflight = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);

        Assert.Equal(StudioReadApiPreflightStatus.Ready, preflight.Status);
        Assert.True(preflight.IsReady);
        Assert.False(preflight.HasFailures);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioApiBaseAddressConfigured).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioWebAuthenticationConfigured).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioApiTokenAcquisitionConfigured).Status);
        Assert.IsType<StudioAuthoringReadApiClient>(provider.GetRequiredService<IStudioContentClient>());
        AssertSafePreflightOutput(preflight, values.Values);
    }

    [Fact]
    public async Task Invalid_configuration_returns_safe_typed_failures()
    {
        var values = BuildValidActivationValues();
        values["StudioApi:BaseAddress"] = CreateInsecureApiBaseAddress();
        values.Remove("Authentication:StudioWeb:ClientId");
        values.Remove("StudioApi:TokenAcquisition:Scopes:0");
        using var provider = BuildProvider(BuildConfiguration(values));

        var preflight = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);

        Assert.Equal(StudioReadApiPreflightStatus.Invalid, preflight.Status);
        Assert.False(preflight.IsReady);
        Assert.True(preflight.HasFailures);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Failed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioApiBaseAddressConfigured).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Failed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioWebAuthenticationConfigured).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Failed,
            Find(preflight, StudioReadApiPreflightCheckCodes.StudioApiTokenAcquisitionConfigured).Status);
        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
        AssertSafePreflightOutput(preflight, values.Values);
    }

    [Fact]
    public async Task Preflight_messages_do_not_contain_fake_sensitive_markers()
    {
        var values = BuildValidActivationValues();
        values["StudioApi:BaseAddress"] = string.Concat(
            CreateSafeApiBaseAddress(),
            "?marker=",
            Forbidden("Password", "="),
            "unsafe");
        values["Authentication:StudioWeb:Authority"] = string.Concat(
            CreateSafeAuthority(),
            "?marker=",
            Forbidden("Bearer", " "));
        using var provider = BuildProvider(BuildConfiguration(values));

        var preflight = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);

        AssertSafePreflightOutput(preflight, values.Values);
    }

    [Fact]
    public async Task Preflight_reports_tenant_and_test_header_guardrails()
    {
        using var provider = BuildProvider(new ConfigurationBuilder().Build());

        var preflight = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);

        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.TenantHeaderNotClientSupplied).Status);
        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.TestHeadersNotProductionShortcuts).Status);
        Assert.True(StudioApiRequestBoundaryHandler.IsProhibitedHeader(
            StudioApiRequestBoundaryHandler.TenantHeaderName));
        Assert.True(StudioApiRequestBoundaryHandler.IsProhibitedHeader(
            string.Concat(StudioApiRequestBoundaryHandler.TestHeaderPrefix, "Authenticated")));
    }

    [Fact]
    public async Task Preflight_confirms_live_write_workflow_and_publish_methods_are_unavailable()
    {
        using var provider = BuildProvider(BuildConfiguration(BuildValidActivationValues()));

        var preflight = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);
        var readClient = provider.GetRequiredService<IStudioContentClient>();

        Assert.Equal(
            StudioReadApiPreflightCheckStatus.Passed,
            Find(preflight, StudioReadApiPreflightCheckCodes.ReadClientIsReadOnly).Status);

        var create = await readClient.CreateDraftAsync(
            new StudioContentCreateDraftRequest(
                Guid.NewGuid(),
                "Guidance title",
                "guidance.title",
                RuntimeContentType.Tooltip,
                "1.0.0"),
            CancellationToken.None);
        var review = await readClient.RequestReviewAsync(
            new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);
        var publish = await readClient.PublishAsync(
            new StudioPublishActionRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "production",
                DeliveryChannel.Published),
            CancellationToken.None);

        Assert.All(
            [create.Status, review.Status, publish.Status],
            status => Assert.Equal(StudioContentClientStatus.Unavailable, status));
    }

    [Fact]
    public void Studio_request_models_do_not_accept_tenant_identifiers()
    {
        var requestTypes = new[]
        {
            typeof(StudioContentListRequest),
            typeof(StudioContentGetByIdRequest),
            typeof(StudioContentCreateDraftRequest),
            typeof(StudioContentUpdateDraftRequest),
            typeof(StudioWorkflowActionRequest),
            typeof(StudioPublishActionRequest)
        };

        Assert.All(requestTypes, requestType =>
        {
            Assert.DoesNotContain(
                requestType.GetProperties(),
                property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void Readiness_docs_include_preflight_topics_and_avoid_secret_markers()
    {
        var guide = ReadRepositoryFile(ReadinessGuidePath);
        var sprint = ReadRepositoryFile(Sprint10Path);
        var docs = string.Concat(guide, Environment.NewLine, sprint);

        Assert.Contains("preflight", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("StudioReadApiActivationValidator", docs, StringComparison.Ordinal);
        Assert.Contains("safe output", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("disabled by default", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fails closed", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live create draft", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live publish", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sprint 10", sprint, StringComparison.Ordinal);
        Assert.Contains("Slice 1", sprint, StringComparison.Ordinal);
        AssertDocsAvoidSecretMarkers(docs);
    }

    private static StudioReadApiPreflightCheck Find(
        StudioReadApiPreflightResult result,
        string code) =>
        Assert.Single(result.Checks, check => string.Equals(check.Code, code, StringComparison.Ordinal));

    private static ServiceProvider BuildProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddStudioApiBoundary(configuration);
        services.AddStudioReadApiActivationGate(configuration);

        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    private static Dictionary<string, string?> BuildValidActivationValues() =>
        new()
        {
            ["StudioApi:Enabled"] = "true",
            ["StudioApi:BaseAddress"] = CreateSafeApiBaseAddress(),
            ["Authentication:StudioWeb:Enabled"] = "true",
            ["Authentication:StudioWeb:Authority"] = CreateSafeAuthority(),
            ["Authentication:StudioWeb:ClientId"] = Guid.NewGuid().ToString(),
            ["Authentication:StudioWeb:CallbackPath"] = "/signin-oidc",
            ["StudioApi:TokenAcquisition:Enabled"] = "true",
            ["StudioApi:TokenAcquisition:Scopes:0"] = CreateSafeScope()
        };

    private static string CreateSafeApiBaseAddress() =>
        new UriBuilder(Uri.UriSchemeHttps, "localhost").Uri.AbsoluteUri;

    private static string CreateInsecureApiBaseAddress() =>
        new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri.AbsoluteUri;

    private static string CreateSafeAuthority() =>
        new UriBuilder(Uri.UriSchemeHttps, "localhost").Uri.AbsoluteUri;

    private static string CreateSafeScope() =>
        $"api://{Guid.NewGuid():D}/access";

    private static void AssertSafePreflightOutput(
        StudioReadApiPreflightResult result,
        IEnumerable<string?> configuredValues)
    {
        Assert.All(result.Checks, check =>
        {
            Assert.False(string.IsNullOrWhiteSpace(check.Code));
            Assert.False(string.IsNullOrWhiteSpace(check.Message));
            foreach (var configuredValue in configuredValues.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                Assert.DoesNotContain(configuredValue!, check.Message, StringComparison.OrdinalIgnoreCase);
            }

            AssertSafeMessage(check.Message);
        });
    }

    private static void AssertSafeMessage(string message)
    {
        foreach (var marker in SensitiveMarkers())
        {
            Assert.DoesNotContain(marker, message, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertDocsAvoidSecretMarkers(string docs)
    {
        Assert.DoesNotContain(Forbidden("Password", "="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("User", " Id="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Account", "Key="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Bearer", " "), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Client", "Secret"), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=tcp:", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Initial Catalog=", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Data Source=", docs, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] SensitiveMarkers() =>
    [
        Forbidden("Password", "="),
        Forbidden("User", " Id="),
        Forbidden("Account", "Key="),
        Forbidden("Bearer", " "),
        Forbidden("Client", "Secret"),
        Forbidden("Connection", "String="),
        "Server=tcp:",
        "Initial Catalog=",
        "Data Source=",
        "HMRC",
        "tax value",
        "property data"
    ];

    private static string Forbidden(string left, string right) =>
        string.Concat(left, right);

    private static string ReadRepositoryFile(string relativePath)
    {
        var repository = FindRepositoryRoot();

        return File.ReadAllText(Path.Combine(repository.FullName, relativePath));
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Adopta.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return directory;
    }
}
