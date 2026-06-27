using Adopta.Application.Abstractions;

namespace Adopta.Api.Tenancy;

public sealed class RequireAdoptaTenantEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<IAdoptionTenantContext>();

        if (!tenantContext.HasTenant)
        {
            return Results.Problem(
                title: "Tenant context required.",
                detail: "A valid tenant context is required for this operation.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}
