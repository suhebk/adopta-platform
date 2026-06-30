using System.Net;
using System.Net.Http.Json;
using System.Text;
using Adopta.Application.Runtime;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioAuthoringReadApiClientTests
{
    [Fact]
    public async Task Api_client_maps_successful_list_response()
    {
        HttpRequestMessage? capturedRequest = null;
        using var httpClient = CreateClient(request =>
        {
            capturedRequest = request;
            return JsonResponse(HttpStatusCode.OK, new StudioAuthoringContentListApiResponse(
                [BuildContentResponse()]));
        });
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("welcome.tooltip", item.ContentKey);
        Assert.False(item.HasKnownContentType);
        Assert.Equal("/authoring/content", capturedRequest?.RequestUri?.AbsolutePath);
        AssertNoTenantInput(capturedRequest);
    }

    [Fact]
    public async Task Api_client_maps_successful_get_response()
    {
        var contentId = Guid.NewGuid();
        using var httpClient = CreateClient(_ =>
            JsonResponse(HttpStatusCode.OK, BuildContentResponse(contentId)));
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.GetByIdAsync(new StudioContentGetByIdRequest(contentId), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(contentId, result.Value.Id);
        Assert.Equal("welcome.tooltip", result.Value.ContentKey);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, StudioContentClientStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, StudioContentClientStatus.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, StudioContentClientStatus.NotFound)]
    public async Task Api_client_maps_standard_failure_status_codes_safely(
        HttpStatusCode statusCode,
        StudioContentClientStatus expectedStatus)
    {
        using var httpClient = CreateClient(_ => new HttpResponseMessage(statusCode));
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Null(result.Value);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Api_client_maps_malformed_response_safely()
    {
        using var httpClient = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{", Encoding.UTF8, "application/json")
        });
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.InvalidResponse, result.Status);
        Assert.Null(result.Value);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Api_client_maps_transport_failure_safely()
    {
        using var httpClient = CreateClient(_ => throw new HttpRequestException("ConnectionString=unsafe"));
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Unavailable, result.Status);
        Assert.Null(result.Value);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Api_client_maps_unexpected_error_without_exposing_exception_details()
    {
        using var httpClient = CreateClient(_ => throw new InvalidOperationException("Bearer token leaked"));
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.GetByIdAsync(new StudioContentGetByIdRequest(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.UnexpectedError, result.Status);
        Assert.Null(result.Value);
        AssertSafeMessage(result.SafeMessage);
        Assert.DoesNotContain("Bearer", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Api_client_rejects_empty_get_request_before_transport()
    {
        var transportCalled = false;
        using var httpClient = CreateClient(_ =>
        {
            transportCalled = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new StudioAuthoringReadApiClient(httpClient);

        var result = await client.GetByIdAsync(new StudioContentGetByIdRequest(Guid.Empty), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.InvalidResponse, result.Status);
        Assert.False(transportCalled);
    }

    [Fact]
    public async Task Api_client_write_methods_are_disabled_safely()
    {
        var client = new StudioAuthoringReadApiClient(CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)));

        var create = await client.CreateDraftAsync(
            new StudioContentCreateDraftRequest(Guid.NewGuid(), "Title", "title.key", null, "0.1.0"),
            CancellationToken.None);
        var review = await client.RequestReviewAsync(
            new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);
        var publish = await client.PublishAsync(
            new StudioPublishActionRequest(Guid.NewGuid(), Guid.NewGuid(), "production", default),
            CancellationToken.None);

        Assert.Equal(StudioContentClientStatus.Unavailable, create.Status);
        Assert.Equal(StudioContentClientStatus.Unavailable, review.Status);
        Assert.Equal(StudioContentClientStatus.Unavailable, publish.Status);
        AssertSafeMessage(create.SafeMessage);
        AssertSafeMessage(review.SafeMessage);
        AssertSafeMessage(publish.SafeMessage);
    }

    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handle)
    {
        return new HttpClient(new StubHttpMessageHandler(handle))
        {
            BaseAddress = new Uri("https://adopta.test")
        };
    }

    private static HttpResponseMessage JsonResponse<T>(
        HttpStatusCode statusCode,
        T body)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(body)
        };
    }

    private static StudioAuthoringContentApiResponse BuildContentResponse(Guid? id = null)
    {
        return new StudioAuthoringContentApiResponse(
            id ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "welcome.tooltip",
            "Welcome tooltip",
            [
                new StudioAuthoringContentVersionApiResponse(
                    Guid.NewGuid(),
                    "1.0.0",
                    StudioAuthoringLifecycleStateApiResponse.Approved,
                    DateTimeOffset.Parse("2026-06-30T10:00:00Z"))
            ]);
    }

    private static void AssertNoTenantInput(HttpRequestMessage? request)
    {
        Assert.NotNull(request);
        Assert.DoesNotContain("tenant", request.RequestUri?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Headers, header => header.Key.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
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
        Assert.DoesNotContain("Bearer", message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> handle;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handle)
        {
            this.handle = handle;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(handle(request));
        }
    }
}
