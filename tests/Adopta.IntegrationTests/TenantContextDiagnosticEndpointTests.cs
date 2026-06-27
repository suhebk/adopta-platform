using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Adopta.IntegrationTests;

public sealed class TenantContextDiagnosticEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TenantContextDiagnosticEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Missing_tenant_context_is_denied_on_tenant_required_endpoint()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/diagnostics/tenant-context");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("Tenant context required", body, StringComparison.Ordinal);
        Assert.DoesNotContain("System.", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Valid_development_tenant_header_allows_diagnostic_endpoint()
    {
        var tenantId = Guid.NewGuid();
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/diagnostics/tenant-context");
        request.Headers.Add("X-Adopta-Tenant-Id", tenantId.ToString());

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(tenantId.ToString(), body, StringComparison.Ordinal);
        Assert.Contains("hasExternalTenantId", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Authorization", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claims", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
    }
}
