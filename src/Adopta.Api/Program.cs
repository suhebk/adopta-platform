using Adopta.Api.Tenancy;
using Adopta.Application;
using Adopta.Application.Abstractions;
using Adopta.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAdoptaApplication();
builder.Services.AddAdoptaInfrastructure();

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
        IApplicationRegistrationService applicationRegistrationService) =>
    {
        _ = tenantContext;
        _ = authorizationService;
        _ = auditService;
        _ = applicationRegistrationService;

        return Results.Ok(new
        {
            status = "ready"
        });
    });

app.Run();

public partial class Program;
