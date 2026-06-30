using Adopta.Application.Runtime;
using Adopta.Web.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.UnitTests;

public sealed class StudioReadApiEnvironmentValidationTests
{
    private const string EnvironmentValidationGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ENVIRONMENT-VALIDATION.md";

    private const string ActivationReadinessGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ACTIVATION-READINESS.md";

    private const string Sprint10Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-10.md";

    [Fact]
    public void Web_appsettings_contain_no_studio_api_activation_values()
    {
        var appsettings = ReadWebAppsettings();

        Assert.All(appsettings, file =>
        {
            Assert.DoesNotContain("StudioApi", file.Content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("TokenAcquisition", file.Content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("BaseAddress", file.Content, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Web_appsettings_contain_no_real_auth_or_token_values()
    {
        var appsettings = ReadWebAppsettings();

        Assert.All(appsettings, file =>
        {
            Assert.DoesNotContain("Authentication:StudioWeb", file.Content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Authority", file.Content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ClientId", file.Content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Scopes", file.Content, StringComparison.OrdinalIgnoreCase);
            AssertAvoidsSecretMarkers(file.Content);
        });
    }

    [Fact]
    public void Default_dependency_injection_resolves_local_client()
    {
        using var provider = BuildProvider(new ConfigurationBuilder().Build());

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
    }

    [Fact]
    public void Invalid_external_style_configuration_fails_closed_to_local_client()
    {
        var values = BuildValidExternalStyleValues();
        values["StudioApi:BaseAddress"] = CreateInsecureEndpointValue();
        values.Remove("StudioApi:TokenAcquisition:Scopes:0");
        using var provider = BuildProvider(BuildConfiguration(values));

        var activation = StudioReadApiActivationValidator.Validate(BuildConfiguration(values));

        Assert.Equal(StudioReadApiActivationStatus.Invalid, activation.Status);
        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
    }

    [Fact]
    public void Valid_external_style_in_memory_configuration_resolves_read_only_api_client()
    {
        using var provider = BuildProvider(BuildConfiguration(BuildValidExternalStyleValues()));

        Assert.IsType<StudioAuthoringReadApiClient>(provider.GetRequiredService<IStudioContentClient>());
    }

    [Fact]
    public async Task Preflight_ready_state_does_not_expose_configured_values()
    {
        var values = BuildValidExternalStyleValues();
        using var provider = BuildProvider(BuildConfiguration(values));

        var result = await provider
            .GetRequiredService<IStudioReadApiPreflightService>()
            .RunAsync(CancellationToken.None);

        Assert.Equal(StudioReadApiPreflightStatus.Ready, result.Status);
        Assert.All(result.Checks, check =>
        {
            Assert.False(string.IsNullOrWhiteSpace(check.Code));
            Assert.False(string.IsNullOrWhiteSpace(check.Message));
            foreach (var configuredValue in values.Values.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                Assert.DoesNotContain(configuredValue!, check.Message, StringComparison.OrdinalIgnoreCase);
            }

            AssertSafeMessage(check.Message);
        });
    }

    [Fact]
    public async Task Read_client_write_workflow_and_publish_methods_remain_unavailable()
    {
        using var provider = BuildProvider(BuildConfiguration(BuildValidExternalStyleValues()));
        var client = provider.GetRequiredService<IStudioContentClient>();

        var create = await client.CreateDraftAsync(
            new StudioContentCreateDraftRequest(
                Guid.NewGuid(),
                "Guidance title",
                "guidance.title",
                RuntimeContentType.Tooltip,
                "1.0.0"),
            CancellationToken.None);
        var update = await client.UpdateDraftAsync(
            new StudioContentUpdateDraftRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Guidance title",
                "guidance.title",
                RuntimeContentType.Tooltip,
                "1.0.0"),
            CancellationToken.None);
        var review = await client.RequestReviewAsync(
            new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);
        var approve = await client.ApproveAsync(
            new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);
        var reject = await client.RejectAsync(
            new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);
        var publish = await client.PublishAsync(
            new StudioPublishActionRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "production",
                DeliveryChannel.Published),
            CancellationToken.None);

        Assert.All(
            [create.Status, update.Status, review.Status, approve.Status, reject.Status, publish.Status],
            status => Assert.Equal(StudioContentClientStatus.Unavailable, status));
    }

    [Fact]
    public void Tenant_identifiers_are_not_accepted_by_page_or_request_models()
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

        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");
        Assert.DoesNotContain("TenantId", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(StudioApiRequestBoundaryHandler.TenantHeaderName, markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(StudioApiRequestBoundaryHandler.TestHeaderPrefix, markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tenant_and_test_headers_are_not_forwarded_by_web_production_code()
    {
        var files = FindRepositoryRoot()
            .GetFiles("*.cs", SearchOption.AllDirectories)
            .Where(file => file.FullName.Contains(Path.Combine("src", "Adopta.Web"), StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.FullName.Contains(Path.Combine("bin"), StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.FullName.Contains(Path.Combine("obj"), StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Name.Equals("StudioApiRequestBoundaryHandler.cs", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(files);
        Assert.All(files, file =>
        {
            var source = File.ReadAllText(file.FullName);
            Assert.DoesNotContain(StudioApiRequestBoundaryHandler.TenantHeaderName, source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(StudioApiRequestBoundaryHandler.TestHeaderPrefix, source, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Environment_validation_guide_exists_and_documents_required_boundaries()
    {
        var guide = ReadRepositoryFile(EnvironmentValidationGuidePath);

        Assert.Contains("secure external configuration", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<https-studio-api-base-address-from-secure-configuration>", guide, StringComparison.Ordinal);
        Assert.Contains("<web-auth-authority-from-secure-configuration>", guide, StringComparison.Ordinal);
        Assert.Contains("<web-client-id-from-secure-configuration>", guide, StringComparison.Ordinal);
        Assert.Contains("<downstream-api-scope-from-secure-configuration>", guide, StringComparison.Ordinal);
        Assert.Contains("default", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fails closed", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live publish", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("X-Adopta-Tenant-Id", guide, StringComparison.Ordinal);
        Assert.Contains("X-Adopta-Test-*", guide, StringComparison.Ordinal);
        AssertAvoidsSecretMarkers(guide);
    }

    [Fact]
    public void Sprint_and_readiness_docs_reference_environment_validation_safely()
    {
        var readiness = ReadRepositoryFile(ActivationReadinessGuidePath);
        var sprint = ReadRepositoryFile(Sprint10Path);
        var docs = string.Concat(readiness, Environment.NewLine, sprint);

        Assert.Contains("STUDIO-READ-API-ENVIRONMENT-VALIDATION.md", docs, StringComparison.Ordinal);
        Assert.Contains("Slice 2", sprint, StringComparison.Ordinal);
        Assert.Contains("Controlled Read-Only Studio API Environment Validation", sprint, StringComparison.Ordinal);
        Assert.Contains("LocalStudioContentClient", docs, StringComparison.Ordinal);
        Assert.Contains("StudioAuthoringReadApiClient", docs, StringComparison.Ordinal);
        AssertAvoidsSecretMarkers(docs);
    }

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

    private static Dictionary<string, string?> BuildValidExternalStyleValues() =>
        new()
        {
            ["StudioApi:Enabled"] = "true",
            ["StudioApi:BaseAddress"] = CreateSecureEndpointValue(),
            ["Authentication:StudioWeb:Enabled"] = "true",
            ["Authentication:StudioWeb:Authority"] = CreateSecureEndpointValue(),
            ["Authentication:StudioWeb:ClientId"] = Guid.NewGuid().ToString(),
            ["Authentication:StudioWeb:CallbackPath"] = "/signin-oidc",
            ["StudioApi:TokenAcquisition:Enabled"] = "true",
            ["StudioApi:TokenAcquisition:Scopes:0"] = CreateSafeScopeValue()
        };

    private static string CreateSecureEndpointValue() =>
        new UriBuilder(Uri.UriSchemeHttps, "localhost").Uri.AbsoluteUri;

    private static string CreateInsecureEndpointValue() =>
        new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri.AbsoluteUri;

    private static string CreateSafeScopeValue() =>
        $"api://{Guid.NewGuid():D}/read";

    private static (string Path, string Content)[] ReadWebAppsettings()
    {
        var webDirectory = Path.Combine(FindRepositoryRoot().FullName, "src", "Adopta.Web");

        return Directory
            .GetFiles(webDirectory, "appsettings*.json", SearchOption.TopDirectoryOnly)
            .Select(path => (path, File.ReadAllText(path)))
            .ToArray();
    }

    private static void AssertSafeMessage(string message)
    {
        foreach (var marker in SensitiveMarkers())
        {
            Assert.DoesNotContain(marker, message, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertAvoidsSecretMarkers(string content)
    {
        foreach (var marker in SensitiveMarkers())
        {
            Assert.DoesNotContain(marker, content, StringComparison.OrdinalIgnoreCase);
        }
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
        "Data Source="
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
