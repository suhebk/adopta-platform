using Adopta.Application.Abstractions;
namespace Adopta.Api.Auth;

public sealed class RequirePermissionEndpointFilter : IEndpointFilter
{
    private readonly string _permissionKey;

    public RequirePermissionEndpointFilter(string permissionKey)
    {
        _permissionKey = permissionKey;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_permissionKey))
        {
            return await ForbiddenAsync(context, "permission_key_missing");
        }

        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<IAdoptionTenantContext>();
        if (!tenantContext.HasTenant)
        {
            return await ForbiddenAsync(context, "tenant_context_missing");
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return await ForbiddenAsync(context, "principal_not_authenticated", tenantContext.TenantId);
        }

        var userMappingService = context.HttpContext.RequestServices.GetRequiredService<IAuthenticatedUserMappingService>();
        var userMapping = userMappingService.MapUser(tenantContext.TenantId, context.HttpContext.User);
        if (!userMapping.IsMapped || userMapping.User is null)
        {
            return await ForbiddenAsync(context, userMapping.FailureCode, tenantContext.TenantId);
        }

        try
        {
            var permissionEvaluator = context.HttpContext.RequestServices.GetRequiredService<IAdoptionPermissionEvaluator>();
            if (!permissionEvaluator.IsAllowed(userMapping.User, _permissionKey))
            {
                return await ForbiddenAsync(context, "permission_denied", tenantContext.TenantId);
            }
        }
        catch
        {
            return await ForbiddenAsync(context, "authorization_failed", tenantContext.TenantId);
        }

        return await next(context);
    }

    private static async Task<IResult> ForbiddenAsync(
        EndpointFilterInvocationContext context,
        string failureCategory,
        Guid? tenantId = null)
    {
        var securityAuditService = context.HttpContext.RequestServices.GetService<IAdoptionSecurityAuditService>();
        if (securityAuditService is not null)
        {
            await securityAuditService.RecordAsync(
                "PermissionCheck",
                "Denied",
                tenantId,
                failureCategory);
        }

        return Forbidden();
    }

    private static IResult Forbidden()
    {
        return Results.Problem(
            title: "Access denied.",
            detail: "The requested operation is not permitted.",
            statusCode: StatusCodes.Status403Forbidden);
    }
}
