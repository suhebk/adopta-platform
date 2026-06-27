using Adopta.Api.Auth;
using Adopta.Api.Tenancy;
using Adopta.Application;
using Adopta.Application.Abstractions;
using Adopta.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAdoptaApplication();
builder.Services.AddAdoptaInfrastructure(builder.Configuration);
builder.Services.Configure<AdoptaAuthenticationOptions>(
    builder.Configuration.GetSection(AdoptaAuthenticationOptions.SectionName));

var app = builder.Build();

app.UseMiddleware<AdoptionTenantContextMiddleware>();

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "live"
}));

app.MapGet(
    "/health/ready",
    (
        IAdoptionTenantContext tenantContext,
        IAdoptionAuthorizationService authorizationService,
        IAdoptionAuditService auditService,
        IApplicationRegistrationService applicationRegistrationService,
        IProductionTenantResolver productionTenantResolver,
        IAdoptionPermissionEvaluator permissionEvaluator) =>
    {
        _ = tenantContext;
        _ = authorizationService;
        _ = auditService;
        _ = applicationRegistrationService;
        _ = productionTenantResolver;
        _ = permissionEvaluator;

        return Results.Ok(new
        {
            status = "ready"
        });
    });

app.MapGet(
    "/diagnostics/tenant-context",
    (IAdoptionTenantContext tenantContext) => Results.Ok(new
    {
        tenantId = tenantContext.TenantId,
        hasExternalTenantId = !string.IsNullOrWhiteSpace(tenantContext.ExternalTenantId)
    }))
    .RequireAdoptaTenantContext();

app.Run();

public partial class Program;
