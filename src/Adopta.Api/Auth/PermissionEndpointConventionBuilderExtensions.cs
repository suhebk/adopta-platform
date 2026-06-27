namespace Adopta.Api.Auth;

public static class PermissionEndpointConventionBuilderExtensions
{
    public static RouteHandlerBuilder RequireAdoptaPermission(
        this RouteHandlerBuilder builder,
        string permissionKey)
    {
        builder.AddEndpointFilter(new RequirePermissionEndpointFilter(permissionKey));

        return builder;
    }
}
