using Adopta.Application.Abstractions;
using Adopta.Domain.Identity;

namespace Adopta.Api.Auth;

public sealed class RequirePermissionEndpointFilter : IEndpointFilter
{
    public const string CurrentUserItemName = "Adopta.CurrentUser";

    private readonly string _permissionKey;

    public RequirePermissionEndpointFilter(string permissionKey)
    {
        _permissionKey = permissionKey;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_permissionKey))
        {
            return Forbidden();
        }

        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<IAdoptionTenantContext>();
        if (!tenantContext.HasTenant)
        {
            return Forbidden();
        }

        if (!context.HttpContext.Items.TryGetValue(CurrentUserItemName, out var currentUserValue)
            || currentUserValue is not AdoptionUser currentUser)
        {
            return Forbidden();
        }

        try
        {
            var permissionEvaluator = context.HttpContext.RequestServices.GetRequiredService<IAdoptionPermissionEvaluator>();
            if (!permissionEvaluator.IsAllowed(currentUser, _permissionKey))
            {
                return Forbidden();
            }
        }
        catch
        {
            return Forbidden();
        }

        return await next(context);
    }

    private static IResult Forbidden()
    {
        return Results.Problem(
            title: "Access denied.",
            detail: "The requested operation is not permitted.",
            statusCode: StatusCodes.Status403Forbidden);
    }
}
