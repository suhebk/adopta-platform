using System.Net;
using System.Net.Http.Json;
using Adopta.Application.Runtime;
using Adopta.Web.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Adopta.UnitTests;

public sealed class StudioReadApiActivationRehearsalTests
{
    private const string RehearsalGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ACTIVATION-REHEARSAL.md";

    private const string EnvironmentValidationGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ENVIRONMENT-VALIDATION.md";

    private const string Sprint10Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-10.md";

    [Fact]
    public async Task Valid_rehearsal_configuration_activates_read_client_and_lists_content_through_boundary()
    {
        var transport = new CapturingTransport();
        var accessValue = CreateFakeAccessValue();
        using var provider = BuildRehearsalProvider(BuildValidActivationValues(), transport, accessValue);
        var client = Assert.IsType<StudioAuthoringReadApiClient>(
            provider.GetRequiredService<IStudioContentClient>());

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.DoesNotContain(accessValue, result.SafeMessage, StringComparison.Ordinal);
        var request = Assert.Single(transport.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/authoring/content", request.RequestUri?.AbsolutePath);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal(accessValue, request.Headers.Authorization?.Parameter);
        AssertNoProhibitedHeaders(request);
    }

    [Fact]
    public async Task Valid_rehearsal_configuration_gets_content_by_id_through_read_only_route()
    {
        var contentId = Guid.NewGuid();
        var transport = new CapturingTransport(contentId);
        using var provider = BuildRehearsalProvider(
            BuildValidActivationValues(),
            transport,
            CreateFakeAccessValue());
        var client = provider.GetRequiredService<IStudioContentClient>();

        var result = await client.GetByIdAsync(
            new StudioContentGetByIdRequest(contentId),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var request = Assert.Single(transport.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"/authoring/content/{contentId:D}", request.RequestUri?.AbsolutePath);
        AssertNoProhibitedHeaders(request);
    }

    [Fact]
    public async Task Request_boundary_strips_prohibited_headers_before_fake_transport_receives_request()
    {
        var transport = new CapturingTransport
        {
            AddProhibitedHeadersBeforeBoundary = true
        };
        using var provider = BuildRehearsalProvider(
            BuildValidActivationValues(),
            transport,
            CreateFakeAccessValue());
        var client = provider.GetRequiredService<IStudioContentClient>();

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.True(result.Succeeded);
        var request = Assert.Single(transport.Requests);
        AssertNoProhibitedHeaders(request);
    }

    [Fact]
    public async Task Activated_read_client_keeps_write_workflow_and_publish_operations_unavailable_without_transport_calls()
    {
        var transport = new CapturingTransport();
        using var provider = BuildRehearsalProvider(
            BuildValidActivationValues(),
            transport,
            CreateFakeAccessValue());
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

        var unavailableResults = new[]
        {
            (create.Status, create.SafeMessage),
            (update.Status, update.SafeMessage),
            (review.Status, review.SafeMessage),
            (approve.Status, approve.SafeMessage),
            (reject.Status, reject.SafeMessage),
            (publish.Status, publish.SafeMessage)
        };
        Assert.All(
            unavailableResults,
            result =>
            {
                Assert.Equal(StudioContentClientStatus.Unavailable, result.Status);
                AssertSafeMessage(result.SafeMessage);
            });
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public void Disabled_configuration_rolls_back_to_local_client()
    {
        var values = BuildValidActivationValues();
        values["StudioApi:Enabled"] = "false";

        using var provider = BuildRehearsalProvider(
            values,
            new CapturingTransport(),
            CreateFakeAccessValue());

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
    }

    [Fact]
    public void Invalid_configuration_fails_closed_to_local_client()
    {
        var values = BuildValidActivationValues();
        values["StudioApi:BaseAddress"] = CreateInsecureEndpointValue();
        values.Remove("StudioApi:TokenAcquisition:Scopes:0");

        using var provider = BuildRehearsalProvider(
            values,
            new CapturingTransport(),
            CreateFakeAccessValue());

        Assert.IsType<LocalStudioContentClient>(provider.GetRequiredService<IStudioContentClient>());
        var activation = StudioReadApiActivationValidator.Validate(BuildConfiguration(values));
        Assert.Equal(StudioReadApiActivationStatus.Invalid, activation.Status);
    }

    [Fact]
    public async Task Missing_fake_server_token_fails_safely_without_forwarding_to_transport()
    {
        var transport = new CapturingTransport();
        using var provider = BuildRehearsalProvider(
            BuildValidActivationValues(),
            transport,
            accessValue: null);
        var client = provider.GetRequiredService<IStudioContentClient>();

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.Equal(StudioContentClientStatus.Unavailable, result.Status);
        AssertSafeMessage(result.SafeMessage);
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public void Rehearsal_docs_exist_and_document_boundaries_without_secret_markers()
    {
        var rehearsal = ReadRepositoryFile(RehearsalGuidePath);
        var environmentValidation = ReadRepositoryFile(EnvironmentValidationGuidePath);
        var sprint = ReadRepositoryFile(Sprint10Path);
        var docs = string.Concat(rehearsal, Environment.NewLine, environmentValidation, Environment.NewLine, sprint);

        Assert.Contains("activation rehearsal", rehearsal, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fake server-side access provider", rehearsal, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fake capturing HTTP transport", rehearsal, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", rehearsal, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Slice 1", rehearsal, StringComparison.Ordinal);
        Assert.Contains("Slice 2", rehearsal, StringComparison.Ordinal);
        Assert.Contains("Slice 3", sprint, StringComparison.Ordinal);
        Assert.Contains("STUDIO-READ-API-ACTIVATION-REHEARSAL.md", environmentValidation, StringComparison.Ordinal);
        AssertAvoidsSecretMarkers(docs);
    }

    private static ServiceProvider BuildRehearsalProvider(
        Dictionary<string, string?> values,
        CapturingTransport transport,
        string? accessValue)
    {
        var configuration = BuildConfiguration(values);
        var services = new ServiceCollection();

        services.AddStudioApiBoundary(configuration);
        services.AddStudioReadApiActivationGate(configuration);
        services.Replace(ServiceDescriptor.Scoped<IStudioApiAccessTokenProvider>(
            _ => new FakeAccessProvider(accessValue)));
        services.Configure<HttpClientFactoryOptions>(
            typeof(StudioAuthoringReadApiClient).Name,
            options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    if (transport.AddProhibitedHeadersBeforeBoundary)
                    {
                        builder.AdditionalHandlers.Insert(0, new TestHeaderInjectionHandler());
                    }

                    builder.PrimaryHandler = transport;
                });
            });

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

    private static string CreateFakeAccessValue() =>
        string.Concat("synthetic-rehearsal-access-", Guid.NewGuid().ToString("N"));

    private static void AssertNoProhibitedHeaders(HttpRequestMessage request)
    {
        Assert.DoesNotContain(
            request.Headers,
            header => StudioApiRequestBoundaryHandler.IsProhibitedHeader(header.Key));
    }

    private static void AssertSafeMessage(string message)
    {
        Assert.False(string.IsNullOrWhiteSpace(message));
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
        "Data Source=",
        "real token",
        "real client",
        "real tenant"
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

    private sealed class FakeAccessProvider : IStudioApiAccessTokenProvider
    {
        private readonly string? accessValue;

        public FakeAccessProvider(string? accessValue)
        {
            this.accessValue = accessValue;
        }

        public Task<StudioApiAccessTokenResult> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                string.IsNullOrWhiteSpace(accessValue)
                    ? StudioApiAccessTokenResult.Unavailable()
                    : StudioApiAccessTokenResult.Available(accessValue));
        }
    }

    private sealed class TestHeaderInjectionHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation(
                StudioApiRequestBoundaryHandler.TenantHeaderName,
                Guid.NewGuid().ToString());
            request.Headers.TryAddWithoutValidation(
                string.Concat(StudioApiRequestBoundaryHandler.TestHeaderPrefix, "Authenticated"),
                "true");

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class CapturingTransport : HttpMessageHandler
    {
        private readonly Guid contentId;
        private readonly List<HttpRequestMessage> requests = [];

        public CapturingTransport(Guid? contentId = null)
        {
            this.contentId = contentId ?? Guid.NewGuid();
        }

        public bool AddProhibitedHeadersBeforeBoundary { get; init; }

        public IReadOnlyList<HttpRequestMessage> Requests => requests;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            requests.Add(CloneRequest(request));

            return Task.FromResult(BuildResponse(request));
        }

        private HttpResponseMessage BuildResponse(HttpRequestMessage request)
        {
            if (request.Method != HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            }

            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (string.Equals(path, "/authoring/content", StringComparison.Ordinal))
            {
                return JsonResponse(new StudioAuthoringContentListApiResponse(
                    [BuildContentResponse(contentId)]));
            }

            if (string.Equals(path, $"/authoring/content/{contentId:D}", StringComparison.Ordinal))
            {
                return JsonResponse(BuildContentResponse(contentId));
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private static HttpResponseMessage JsonResponse<T>(T body) =>
            new(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(body)
            };

        private static StudioAuthoringContentApiResponse BuildContentResponse(Guid contentId) =>
            new(
                contentId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                StudioAuthoringContentTypeApiResponse.Tooltip,
                "welcome.tooltip",
                "Welcome tooltip",
                [
                    new StudioAuthoringContentVersionApiResponse(
                        Guid.NewGuid(),
                        "1.0.0",
                        StudioAuthoringLifecycleStateApiResponse.Approved,
                        DateTimeOffset.Parse("2026-06-30T10:00:00Z"))
                ],
                null);

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
