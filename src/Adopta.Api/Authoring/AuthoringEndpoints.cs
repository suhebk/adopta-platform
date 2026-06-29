using Adopta.Api.Auth;
using Adopta.Api.Tenancy;
using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Authoring;
using Adopta.Application.Identity;
using Adopta.Domain.Authoring;
using Adopta.Infrastructure.Persistence;

namespace Adopta.Api.Authoring;

public static class AuthoringEndpoints
{
    public static IEndpointRouteBuilder MapAuthoringEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/authoring/content", CreateAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringManage);

        app.MapGet("/authoring/content/{contentId:guid}", GetByIdAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringRead);

        app.MapGet("/authoring/content", ListAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringRead);

        app.MapPost("/authoring/content/{contentId:guid}/versions/{versionId:guid}/request-review", RequestReviewAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringReview);

        app.MapPost("/authoring/content/{contentId:guid}/versions/{versionId:guid}/approve", ApproveAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringApprove);

        app.MapPost("/authoring/content/{contentId:guid}/versions/{versionId:guid}/reject", RejectAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringReview);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateAuthoredContentRequest request,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        CancellationToken cancellationToken)
    {
        var contentId = Guid.NewGuid();
        var version = new AuthoredContentVersion(
            Guid.NewGuid(),
            request.Version,
            ContentLifecycleState.Draft,
            DateTimeOffset.UtcNow);

        var validation = AuthoredContentValidator.ValidateContent(new AuthoredContentContract(
            contentId,
            tenantContext.TenantId,
            request.ApplicationId,
            request.ContentKey,
            request.Title,
            [
                new AuthoredContentVersionContract(
                    version.Id,
                    version.Version,
                    version.LifecycleState,
                    version.CreatedAtUtc)
            ]));

        if (!validation.IsValid)
        {
            return Results.BadRequest(CommandFailed("invalid_authored_content", validation.Issues));
        }

        var content = new AuthoredContentItem(
            contentId,
            tenantContext.TenantId,
            request.ApplicationId,
            request.ContentKey.Trim(),
            request.Title.Trim(),
            [version]);

        await repository.AddAsync(content, cancellationToken);

        return Results.Created($"/authoring/content/{content.Id}", CommandSucceeded("created", ToResponse(content), null));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid contentId,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        CancellationToken cancellationToken)
    {
        var content = await repository.GetByIdAsync(tenantContext.TenantId, contentId, cancellationToken);

        return content is null
            ? Results.NotFound(CommandFailed("not_found", [Issue("authored_content_not_found", "content", "Authored content was not found.")]))
            : Results.Ok(ToResponse(content));
    }

    private static async Task<IResult> ListAsync(
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        CancellationToken cancellationToken)
    {
        var content = await repository.ListAsync(tenantContext.TenantId, cancellationToken);

        return Results.Ok(new AuthoredContentListResponse(content.Select(ToResponse).ToArray()));
    }

    private static Task<IResult> RequestReviewAsync(
        Guid contentId,
        Guid versionId,
        RequestReviewRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        CancellationToken cancellationToken)
    {
        var actorUserId = ResolveActorUserId(httpContext, tenantContext);
        var workflow = new AuthoredContentApprovalWorkflow();

        return RunWorkflowAsync(
            () => workflow.RequestReviewAsync(
                repository,
                new AuthoredContentReviewRequest(
                    tenantContext.TenantId,
                    contentId,
                    versionId,
                    actorUserId,
                    request?.RequestedAtUtc ?? DateTimeOffset.UtcNow),
                cancellationToken));
    }

    private static Task<IResult> ApproveAsync(
        Guid contentId,
        Guid versionId,
        ApprovalDecisionRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        CancellationToken cancellationToken)
    {
        return DecideAsync(
            contentId,
            versionId,
            request,
            httpContext,
            tenantContext,
            repository,
            AuthoredContentApprovalDecisionKind.Approve,
            cancellationToken);
    }

    private static Task<IResult> RejectAsync(
        Guid contentId,
        Guid versionId,
        ApprovalDecisionRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        CancellationToken cancellationToken)
    {
        return DecideAsync(
            contentId,
            versionId,
            request,
            httpContext,
            tenantContext,
            repository,
            AuthoredContentApprovalDecisionKind.Reject,
            cancellationToken);
    }

    private static Task<IResult> DecideAsync(
        Guid contentId,
        Guid versionId,
        ApprovalDecisionRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        AuthoredContentApprovalDecisionKind decision,
        CancellationToken cancellationToken)
    {
        var actorUserId = ResolveActorUserId(httpContext, tenantContext);
        var workflow = new AuthoredContentApprovalWorkflow();

        return RunWorkflowAsync(
            () => workflow.DecideAsync(
                repository,
                new AuthoredContentApprovalDecision(
                    tenantContext.TenantId,
                    contentId,
                    versionId,
                    actorUserId,
                    decision,
                    request?.DecidedAtUtc ?? DateTimeOffset.UtcNow),
                cancellationToken));
    }

    private static async Task<IResult> RunWorkflowAsync(
        Func<Task<AuthoredContentLifecycleDecisionResult>> action)
    {
        try
        {
            var result = await action();

            return result.Status switch
            {
                AuthoredContentLifecycleDecisionStatus.Succeeded => Results.Ok(new AuthoringCommandResponse(
                    true,
                    "succeeded",
                    null,
                    ToAuditResponse(result.AuditRecord),
                    [])),
                AuthoredContentLifecycleDecisionStatus.NotFound => Results.NotFound(CommandFailed("not_found", result.Issues)),
                _ => Results.BadRequest(CommandFailed("invalid_lifecycle_command", result.Issues))
            };
        }
        catch (TenantAccessDeniedException)
        {
            return Results.NotFound(CommandFailed("not_found", [Issue("authored_content_not_found", "content", "Authored content was not found.")]));
        }
    }

    private static Guid ResolveActorUserId(
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext)
    {
        var userMappingService = httpContext.RequestServices.GetRequiredService<IAuthenticatedUserMappingService>();
        var userMapping = userMappingService.MapUser(tenantContext.TenantId, httpContext.User);

        return userMapping.User?.Id ?? Guid.Empty;
    }

    private static AuthoredContentResponse ToResponse(AuthoredContentItem content)
    {
        return new AuthoredContentResponse(
            content.Id,
            content.TenantId,
            content.ApplicationId,
            content.ContentKey,
            content.Title,
            content.Versions.Select(ToVersionResponse).ToArray());
    }

    private static AuthoredContentVersionResponse ToVersionResponse(AuthoredContentVersion version)
    {
        return new AuthoredContentVersionResponse(
            version.Id,
            version.Version,
            version.LifecycleState,
            version.CreatedAtUtc);
    }

    private static LifecycleDecisionAuditResponse? ToAuditResponse(AuthoredContentLifecycleAuditRecord? audit)
    {
        return audit is null
            ? null
            : new LifecycleDecisionAuditResponse(
                audit.TenantId,
                audit.ContentId,
                audit.VersionId,
                audit.ActorUserId,
                audit.LifecycleAction,
                audit.FromState,
                audit.ToState,
                audit.Result,
                audit.OccurredAtUtc);
    }

    private static AuthoringCommandResponse CommandSucceeded(
        string status,
        AuthoredContentResponse? content,
        LifecycleDecisionAuditResponse? audit)
    {
        return new AuthoringCommandResponse(true, status, content, audit, []);
    }

    private static AuthoringCommandResponse CommandFailed(
        string status,
        IReadOnlyCollection<AuthoredContentValidationIssue> issues)
    {
        return new AuthoringCommandResponse(
            false,
            status,
            null,
            null,
            issues.Select(issue => new AuthoringIssueResponse(issue.Code, issue.Path, issue.Message)).ToArray());
    }

    private static AuthoredContentValidationIssue Issue(
        string code,
        string path,
        string message)
    {
        return new AuthoredContentValidationIssue(code, path, message);
    }
}
