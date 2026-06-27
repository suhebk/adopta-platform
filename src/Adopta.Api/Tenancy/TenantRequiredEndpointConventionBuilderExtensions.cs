namespace Adopta.Api.Tenancy;

public static class TenantRequiredEndpointConventionBuilderExtensions
{
    public static RouteHandlerBuilder RequireAdoptaTenantContext(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<RequireAdoptaTenantEndpointFilter>();

        return builder;
    }
}
