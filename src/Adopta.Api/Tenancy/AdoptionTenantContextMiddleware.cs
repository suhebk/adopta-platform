using Adopta.Infrastructure.Tenancy;

namespace Adopta.Api.Tenancy;

public sealed class AdoptionTenantContextMiddleware
{
    public const string DevelopmentTenantHeaderName = "X-Adopta-Tenant-Id";

    private readonly RequestDelegate _next;

    public AdoptionTenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, AdoptionTenantContext tenantContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(DevelopmentTenantHeaderName, out var tenantHeader))
        {
            await _next(httpContext);
            return;
        }

        // Sprint 1 development/test placeholder only. This header is not a production
        // trust boundary; production tenant resolution must be derived from validated
        // Microsoft Entra token claims and server-side tenant mappings.
        if (!Guid.TryParse(tenantHeader.ToString(), out var tenantId) || tenantId == Guid.Empty)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                error = "invalid_tenant_context",
                message = "The supplied tenant context is invalid."
            });
            return;
        }

        tenantContext.SetTenant(tenantId);

        await _next(httpContext);
    }
}
