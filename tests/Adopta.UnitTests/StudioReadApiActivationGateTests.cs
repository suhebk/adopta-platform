using Adopta.Application.Runtime;
using Adopta.Web.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.UnitTests;

public sealed class StudioReadApiActivationGateTests
{
    [Fact]
    public void Default_configuration_resolves_local_studio_content_client()
    {
        using var provider = BuildProvider(new ConfigurationBuilder().Build());

        var client = provider.GetRequiredService<IStudioContentClient>();
        var activation = StudioReadApiActivationValidator.Validate(new ConfigurationBuilder().Build());

        Assert.IsType<LocalStudioContentClient>(client);
        Assert.Equal(StudioReadApiActivationStatus.Disabled, activation.Status);
        Assert.Empty(activation.Issues);
    }

    [Fact]
    public void Api_disabled_resolves_local_studio_content_client()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["StudioApi:Enabled"] = "false",
            ["StudioApi:BaseAddress"] = CreateSafeApiBaseAddress()
        });

        using var provider = BuildProvider(configuration);

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
    }

    [Fact]
    public void Api_enabled_without_valid_base_address_resolves_local_studio_content_client()
    {
        var values = BuildValidActivationValues();
        values["StudioApi:BaseAddress"] = CreateInsecureApiBaseAddress();
        var configuration = BuildConfiguration(values);

        using var provider = BuildProvider(configuration);
        var activation = StudioReadApiActivationValidator.Validate(configuration);

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
        Assert.Equal(StudioReadApiActivationStatus.Invalid, activation.Status);
        Assert.Contains(activation.Issues, issue => issue.Code == "studio_api_base_address_required");
        AssertSafeIssues(activation.Issues, values.Values);
    }

    [Fact]
    public void Api_enabled_without_web_authentication_prerequisites_resolves_local_studio_content_client()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["StudioApi:Enabled"] = "true",
            ["StudioApi:BaseAddress"] = CreateSafeApiBaseAddress()
        });

        using var provider = BuildProvider(configuration);
        var activation = StudioReadApiActivationValidator.Validate(configuration);

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
        Assert.Equal(StudioReadApiActivationStatus.Invalid, activation.Status);
        Assert.Contains(activation.Issues, issue => issue.Code == "studio_web_authentication_required");
        Assert.Contains(activation.Issues, issue => issue.Code == "studio_api_access_required");
        AssertSafeIssues(activation.Issues, []);
    }

    [Fact]
    public void Api_enabled_without_token_acquisition_prerequisites_resolves_local_studio_content_client()
    {
        var values = BuildValidActivationValues();
        values.Remove("StudioApi:TokenAcquisition:Enabled");
        values.Remove("StudioApi:TokenAcquisition:Scopes:0");
        var configuration = BuildConfiguration(values);

        using var provider = BuildProvider(configuration);
        var activation = StudioReadApiActivationValidator.Validate(configuration);

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
        Assert.Equal(StudioReadApiActivationStatus.Invalid, activation.Status);
        Assert.Contains(activation.Issues, issue => issue.Code == "studio_api_access_required");
        AssertSafeIssues(activation.Issues, values.Values);
    }

    [Fact]
    public void Valid_explicit_configuration_resolves_studio_authoring_read_api_client()
    {
        var configuration = BuildConfiguration(BuildValidActivationValues());

        using var provider = BuildProvider(configuration);
        var client = provider.GetRequiredService<IStudioContentClient>();
        var activation = StudioReadApiActivationValidator.Validate(configuration);

        Assert.IsType<StudioAuthoringReadApiClient>(client);
        Assert.True(activation.CanActivate);
        Assert.Empty(activation.Issues);
    }

    [Fact]
    public void Activated_client_pipeline_uses_studio_api_request_boundary_handler()
    {
        var extensionSource = ReadRepositoryFile("src/Adopta.Web/Studio/StudioApiServiceCollectionExtensions.cs");

        Assert.Contains("AddHttpClient<StudioAuthoringReadApiClient>", extensionSource, StringComparison.Ordinal);
        Assert.Contains("AddHttpMessageHandler<StudioApiRequestBoundaryHandler>", extensionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("X-Adopta-Tenant-Id", extensionSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("X-Adopta-Test-", extensionSource, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Activated_client_keeps_write_workflow_and_publish_methods_unavailable()
    {
        using var provider = BuildProvider(BuildConfiguration(BuildValidActivationValues()));
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
            new StudioPublishActionRequest(Guid.NewGuid(), Guid.NewGuid(), "production", DeliveryChannel.Published),
            CancellationToken.None);

        Assert.All(
            [create.Status, update.Status, review.Status, approve.Status, reject.Status, publish.Status],
            status => Assert.Equal(StudioContentClientStatus.Unavailable, status));
    }

    [Fact]
    public void Studio_request_and_page_models_do_not_accept_tenant_identifiers()
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

    private static void AssertSafeIssues(
        IEnumerable<StudioReadApiActivationValidationIssue> issues,
        IEnumerable<string?> configuredValues)
    {
        Assert.All(issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Code));
            AssertSafeMessage(issue.Message);
            foreach (var configuredValue in configuredValues.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                Assert.DoesNotContain(configuredValue!, issue.Message, StringComparison.OrdinalIgnoreCase);
            }
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
        Assert.DoesNotContain("tenant", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("authority", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("client", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("scope", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", message, StringComparison.OrdinalIgnoreCase);
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
