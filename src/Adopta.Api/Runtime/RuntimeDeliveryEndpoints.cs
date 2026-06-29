using Adopta.Api.Auth;
using Adopta.Api.Tenancy;
using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Runtime;
using Adopta.Application.Identity;
using Adopta.Application.Runtime;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Runtime;

public static class RuntimeDeliveryEndpoints
{
    public static IEndpointRouteBuilder MapRuntimeDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/runtime/delivery/bundles/{applicationId:guid}", GetBundleAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.RuntimeDeliveryRead);

        return app;
    }

    private static async Task<IResult> GetBundleAsync(
        Guid applicationId,
        [FromQuery] string? environment,
        [FromQuery] string? channel,
        IAdoptionTenantContext tenantContext,
        IDeliveryBundleRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var parseIssues = ValidateRequest(applicationId, environment, channel, out var parsedChannel);
            if (parseIssues.Count > 0)
            {
                return Results.BadRequest(Failed("invalid_delivery_bundle_lookup_request", parseIssues));
            }

            var result = await repository.LookupAsync(
                new DeliveryBundleLookupRequest(
                    tenantContext.TenantId,
                    applicationId,
                    environment!.Trim(),
                    parsedChannel!.Value),
                cancellationToken);

            return result.Status switch
            {
                DeliveryBundleLookupStatus.Found => Results.Ok(Succeeded(result.Bundle!)),
                DeliveryBundleLookupStatus.InvalidRequest => Results.BadRequest(Failed("invalid_delivery_bundle_lookup_request", result.Issues)),
                DeliveryBundleLookupStatus.ValidationFailed => Results.BadRequest(Failed("delivery_bundle_validation_failed", result.Issues)),
                DeliveryBundleLookupStatus.NotFound => Results.NotFound(Failed("not_found", [])),
                DeliveryBundleLookupStatus.AccessDenied => Results.NotFound(Failed("not_found", [])),
                _ => Results.NotFound(Failed("not_found", []))
            };
        }
        catch
        {
            return Results.Problem(
                title: "Delivery bundle lookup failed.",
                detail: "The requested delivery bundle could not be retrieved.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateRequest(
        Guid applicationId,
        string? environment,
        string? channel,
        out DeliveryChannel? parsedChannel)
    {
        var issues = new List<RuntimeContentValidationIssue>();
        parsedChannel = null;

        if (applicationId == Guid.Empty)
        {
            issues.Add(Issue("request.applicationId", "Application id is required."));
        }

        if (string.IsNullOrWhiteSpace(environment))
        {
            issues.Add(Issue("request.environment", "Environment is required."));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            issues.Add(Issue("request.channel", "Channel is required."));
        }
        else if (!Enum.TryParse<DeliveryChannel>(channel, ignoreCase: true, out var parsed) ||
            !Enum.IsDefined(parsed))
        {
            issues.Add(Issue("request.channel", "Channel is invalid."));
        }
        else
        {
            parsedChannel = parsed;
        }

        return issues;
    }

    private static RuntimeDeliveryBundleResponse Succeeded(DeliveryBundle bundle)
    {
        return new RuntimeDeliveryBundleResponse(
            true,
            "found",
            new DeliveryBundleResponse(
                bundle.TenantId,
                bundle.ApplicationId,
                bundle.Environment,
                bundle.Channel,
                bundle.Content),
            []);
    }

    private static RuntimeDeliveryBundleResponse Failed(
        string status,
        IReadOnlyCollection<RuntimeContentValidationIssue> issues)
    {
        return new RuntimeDeliveryBundleResponse(
            false,
            status,
            null,
            issues.Select(issue => new RuntimeDeliveryIssueResponse(issue.Code, issue.Path, issue.Message)).ToArray());
    }

    private static RuntimeContentValidationIssue Issue(string path, string message)
    {
        return new RuntimeContentValidationIssue(
            "invalid_delivery_bundle_lookup_request",
            path,
            message);
    }
}
