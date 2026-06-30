using Adopta.Web.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adopta.UnitTests;

public sealed class StudioApiConfigurationTests
{
    [Fact]
    public void Boundary_services_are_disabled_and_unavailable_by_default()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddStudioApiBoundary(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<StudioApiClientOptions>>().Value;
        var tokenProvider = provider.GetRequiredService<IStudioApiAccessTokenProvider>();
        var handler = provider.GetRequiredService<StudioApiRequestBoundaryHandler>();

        Assert.False(options.Enabled);
        Assert.Equal(string.Empty, options.BaseAddress);
        Assert.False(options.HasConfiguredBaseAddress);
        Assert.IsType<UnavailableStudioApiAccessTokenProvider>(tokenProvider);
        Assert.NotNull(handler);
    }

    [Fact]
    public void Explicit_options_require_https_base_address_before_future_activation()
    {
        Assert.True(new StudioApiClientOptions
        {
            Enabled = true,
            BaseAddress = "https://localhost"
        }.HasConfiguredBaseAddress);

        Assert.False(new StudioApiClientOptions
        {
            Enabled = true,
            BaseAddress = "http://localhost"
        }.HasConfiguredBaseAddress);

        Assert.False(new StudioApiClientOptions
        {
            Enabled = true,
            BaseAddress = string.Empty
        }.HasConfiguredBaseAddress);
    }

    [Fact]
    public void Program_registers_boundary_and_activation_gate_without_direct_live_client_activation()
    {
        var program = ReadRepositoryFile("src/Adopta.Web/Program.cs");

        Assert.Contains("builder.Services.AddStudioApiBoundary(builder.Configuration);", program, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddStudioReadApiActivationGate(builder.Configuration);", program, StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.AddScoped<IStudioContentClient, LocalStudioContentClient>();", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddScoped<IStudioContentClient, StudioAuthoringReadApiClient>", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHttpClient<IStudioContentClient", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHttpClient<StudioAuthoringReadApiClient", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_still_avoids_live_api_activation()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("@inject IStudioContentClient StudioContentClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("StudioAuthoringReadApiClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("/authoring/content", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_page_and_request_models_do_not_accept_tenant_identifiers()
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
    public void Read_api_client_does_not_introduce_live_write_workflow_or_publish_calls()
    {
        var client = ReadRepositoryFile("src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs");

        Assert.DoesNotContain("PostAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PutAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PatchAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("DeleteAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("/request-review", client, StringComparison.Ordinal);
        Assert.DoesNotContain("/approve", client, StringComparison.Ordinal);
        Assert.DoesNotContain("/reject", client, StringComparison.Ordinal);
        Assert.DoesNotContain("/publish", client, StringComparison.Ordinal);
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
