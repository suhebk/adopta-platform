using System.Net;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioApiAuthBoundaryTests
{
    [Fact]
    public async Task Default_token_provider_returns_safe_unavailable_result()
    {
        var provider = new UnavailableStudioApiAccessTokenProvider();

        var result = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(StudioApiAccessTokenStatus.Unavailable, result.Status);
        Assert.False(result.HasAccessToken);
        Assert.Null(result.AccessToken);
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Handler_does_not_add_authorization_when_token_is_unavailable()
    {
        var innerHandler = new CapturingHandler();
        using var handler = new StudioApiRequestBoundaryHandler(
            new StaticTokenProvider(StudioApiAccessTokenResult.Unavailable()))
        {
            InnerHandler = innerHandler
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/authoring/content");

        using var response = await invoker.SendAsync(request, CancellationToken.None);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.False(innerHandler.WasCalled);
        Assert.Null(request.Headers.Authorization);
        AssertSafeMessage(body);
    }

    [Fact]
    public async Task Handler_adds_authorization_only_when_server_provider_returns_valid_token()
    {
        var innerHandler = new CapturingHandler();
        using var handler = new StudioApiRequestBoundaryHandler(
            new StaticTokenProvider(StudioApiAccessTokenResult.Available(BuildOpaqueAccessValue())))
        {
            InnerHandler = innerHandler
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/authoring/content");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(innerHandler.WasCalled);
        Assert.NotNull(innerHandler.CapturedRequest);
        Assert.Equal("Bearer", innerHandler.CapturedRequest.Headers.Authorization?.Scheme);
        Assert.False(string.IsNullOrWhiteSpace(innerHandler.CapturedRequest.Headers.Authorization?.Parameter));
    }

    [Fact]
    public async Task Handler_strips_tenant_and_test_headers_before_forwarding()
    {
        var innerHandler = new CapturingHandler();
        using var handler = new StudioApiRequestBoundaryHandler(
            new StaticTokenProvider(StudioApiAccessTokenResult.Available(BuildOpaqueAccessValue())))
        {
            InnerHandler = innerHandler
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/authoring/content");
        request.Headers.Add(StudioApiRequestBoundaryHandler.TenantHeaderName, Guid.NewGuid().ToString());
        request.Headers.Add("X-Adopta-Test-Authenticated", "true");
        request.Headers.Add("X-Adopta-Test-Tid", Guid.NewGuid().ToString());
        request.Headers.Add("X-Adopta-Trace", "safe-correlation");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(innerHandler.CapturedRequest);
        Assert.DoesNotContain(
            innerHandler.CapturedRequest.Headers,
            header => StudioApiRequestBoundaryHandler.IsProhibitedHeader(header.Key));
        Assert.Contains(
            innerHandler.CapturedRequest.Headers,
            header => string.Equals(header.Key, "X-Adopta-Trace", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handler_rejects_invalid_access_value_without_leaking_it()
    {
        const string unsafeAccessValue = "unsafe\r\nConnectionString=unsafe";
        var innerHandler = new CapturingHandler();
        using var handler = new StudioApiRequestBoundaryHandler(
            new StaticTokenProvider(StudioApiAccessTokenResult.Available(unsafeAccessValue)))
        {
            InnerHandler = innerHandler
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/authoring/content");

        using var response = await invoker.SendAsync(request, CancellationToken.None);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.False(innerHandler.WasCalled);
        Assert.DoesNotContain("ConnectionString", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("unsafe", body, StringComparison.OrdinalIgnoreCase);
        AssertSafeMessage(body);
    }

    [Theory]
    [InlineData("X-Adopta-Tenant-Id")]
    [InlineData("x-adopta-tenant-id")]
    [InlineData("X-Adopta-Test-Authenticated")]
    [InlineData("X-Adopta-Test-Tid")]
    [InlineData("x-adopta-test-oid")]
    public void Prohibited_headers_cover_tenant_and_test_auth_headers(string headerName)
    {
        Assert.True(StudioApiRequestBoundaryHandler.IsProhibitedHeader(headerName));
    }

    [Fact]
    public void Strip_prohibited_headers_leaves_safe_custom_headers()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/authoring/content");
        request.Headers.Add(StudioApiRequestBoundaryHandler.TenantHeaderName, Guid.NewGuid().ToString());
        request.Headers.Add("X-Adopta-Test-AppId", Guid.NewGuid().ToString());
        request.Headers.Add("X-Adopta-Trace", "safe-correlation");

        var removed = StudioApiRequestBoundaryHandler.StripProhibitedHeaders(request);

        Assert.Equal(2, removed);
        Assert.DoesNotContain(request.Headers, header => StudioApiRequestBoundaryHandler.IsProhibitedHeader(header.Key));
        Assert.Contains(request.Headers, header => string.Equals(header.Key, "X-Adopta-Trace", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildOpaqueAccessValue() =>
        Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace('+', 'a')
            .Replace('/', 'b')
            .TrimEnd('=');

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

    private sealed class StaticTokenProvider : IStudioApiAccessTokenProvider
    {
        private readonly StudioApiAccessTokenResult result;

        public StaticTokenProvider(StudioApiAccessTokenResult result)
        {
            this.result = result;
        }

        public Task<StudioApiAccessTokenResult> GetAccessTokenAsync(CancellationToken cancellationToken) =>
            Task.FromResult(result);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public bool WasCalled { get; private set; }

        public HttpRequestMessage? CapturedRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            CapturedRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
