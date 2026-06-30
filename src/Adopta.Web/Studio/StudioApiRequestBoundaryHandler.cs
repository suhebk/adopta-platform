using System.Net;
using System.Net.Http.Headers;

namespace Adopta.Web.Studio;

public sealed class StudioApiRequestBoundaryHandler : DelegatingHandler
{
    public const string TenantHeaderName = "X-Adopta-Tenant-Id";
    public const string TestHeaderPrefix = "X-Adopta-Test-";

    private readonly IStudioApiAccessTokenProvider accessTokenProvider;

    public StudioApiRequestBoundaryHandler(IStudioApiAccessTokenProvider accessTokenProvider)
    {
        this.accessTokenProvider = accessTokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        StripProhibitedHeaders(request);

        var accessToken = await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (!accessToken.HasAccessToken || !IsSafeHeaderValue(accessToken.AccessToken))
        {
            request.Headers.Authorization = null;
            return SafeUnavailableResponse();
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken!);

        return await base.SendAsync(request, cancellationToken);
    }

    public static int StripProhibitedHeaders(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var removed = 0;
        foreach (var headerName in request.Headers.Select(header => header.Key).ToArray())
        {
            if (!IsProhibitedHeader(headerName))
            {
                continue;
            }

            if (request.Headers.Remove(headerName))
            {
                removed++;
            }
        }

        return removed;
    }

    public static bool IsProhibitedHeader(string headerName) =>
        string.Equals(headerName, TenantHeaderName, StringComparison.OrdinalIgnoreCase)
        || headerName.StartsWith(TestHeaderPrefix, StringComparison.OrdinalIgnoreCase);

    private static bool IsSafeHeaderValue(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.IndexOfAny(['\r', '\n']) < 0;

    private static HttpResponseMessage SafeUnavailableResponse() =>
        new(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("Studio API authentication is unavailable.")
        };
}
