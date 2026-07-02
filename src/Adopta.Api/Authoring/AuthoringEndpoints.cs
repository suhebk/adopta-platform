using Adopta.Api.Auth;
using Adopta.Api.Tenancy;
using Adopta.Application.Abstractions;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Abstractions.Runtime;
using Adopta.Application.Authoring;
using Adopta.Application.Identity;
using Adopta.Application.Runtime;
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

        app.MapPost("/authoring/content/{contentId:guid}/versions/{versionId:guid}/publish", PublishAsync)
            .RequireAdoptaTenantContext()
            .RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringPublish);

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
            request.ContentType,
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
            request.ContentType!.Value,
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
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
        IAuthoredContentPublishingHistoryRepository publishingHistoryRepository,
        CancellationToken cancellationToken)
    {
        var content = await repository.GetByIdAsync(tenantContext.TenantId, contentId, cancellationToken);
        if (content is null)
        {
            return Results.NotFound(CommandFailed("not_found", [Issue("authored_content_not_found", "content", "Authored content was not found.")]));
        }

        var lifecycleHistory = await lifecycleHistoryRepository.ListAsync(cancellationToken);
        var publishingHistory = await publishingHistoryRepository.ListAsync(cancellationToken);

        return Results.Ok(ToResponse(
            content,
            BuildReadSummary(content.TenantId, content.Id, lifecycleHistory, publishingHistory)));
    }

    private static async Task<IResult> ListAsync(
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
        IAuthoredContentPublishingHistoryRepository publishingHistoryRepository,
        CancellationToken cancellationToken)
    {
        var content = await repository.ListAsync(tenantContext.TenantId, cancellationToken);
        var lifecycleHistory = await lifecycleHistoryRepository.ListAsync(cancellationToken);
        var publishingHistory = await publishingHistoryRepository.ListAsync(cancellationToken);

        return Results.Ok(new AuthoredContentListResponse(content
            .Select(item => ToResponse(
                item,
                BuildReadSummary(item.TenantId, item.Id, lifecycleHistory, publishingHistory)))
            .ToArray()));
    }

    private static Task<IResult> RequestReviewAsync(
        Guid contentId,
        Guid versionId,
        RequestReviewRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
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
                cancellationToken),
            lifecycleHistoryRepository,
            cancellationToken);
    }

    private static Task<IResult> ApproveAsync(
        Guid contentId,
        Guid versionId,
        ApprovalDecisionRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
        CancellationToken cancellationToken)
    {
        return DecideAsync(
            contentId,
            versionId,
            request,
            httpContext,
            tenantContext,
            repository,
            lifecycleHistoryRepository,
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
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
        CancellationToken cancellationToken)
    {
        return DecideAsync(
            contentId,
            versionId,
            request,
            httpContext,
            tenantContext,
            repository,
            lifecycleHistoryRepository,
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
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
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
                cancellationToken),
            lifecycleHistoryRepository,
            cancellationToken);
    }

    private static async Task<IResult> RunWorkflowAsync(
        Func<Task<AuthoredContentLifecycleDecisionResult>> action,
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await action();

            return result.Status switch
            {
                AuthoredContentLifecycleDecisionStatus.Succeeded => await PersistLifecycleDecisionAsync(
                    lifecycleHistoryRepository,
                    result,
                    cancellationToken),
                AuthoredContentLifecycleDecisionStatus.NotFound => Results.NotFound(CommandFailed("not_found", result.Issues)),
                _ => Results.BadRequest(CommandFailed("invalid_lifecycle_command", result.Issues))
            };
        }
        catch (TenantAccessDeniedException)
        {
            return Results.NotFound(CommandFailed("not_found", [Issue("authored_content_not_found", "content", "Authored content was not found.")]));
        }
        catch
        {
            return Results.Problem(
                title: "Authoring command failed.",
                detail: "The requested authoring command could not be completed.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> PersistLifecycleDecisionAsync(
        IAuthoredContentLifecycleHistoryRepository lifecycleHistoryRepository,
        AuthoredContentLifecycleDecisionResult result,
        CancellationToken cancellationToken)
    {
        if (result.AuditRecord is not null)
        {
            await lifecycleHistoryRepository.AddAsync(result.AuditRecord, cancellationToken);
        }

        return Results.Ok(new AuthoringCommandResponse(
            true,
            "succeeded",
            null,
            ToAuditResponse(result.AuditRecord),
            []));
    }

    private static async Task<IResult> PublishAsync(
        Guid contentId,
        Guid versionId,
        PublishAuthoredContentRequest? request,
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext,
        IAuthoredContentRepository repository,
        IAuthoredContentPublishingHistoryRepository publishingHistoryRepository,
        IDeliveryBundleRepository deliveryBundleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            request ??= new PublishAuthoredContentRequest("", (DeliveryChannel)int.MinValue, null);

            var actorUserId = ResolveActorUserId(httpContext, tenantContext);
            var workflow = new AuthoredContentPublishingWorkflow();
            var result = await workflow.PublishAsync(
                repository,
                new AuthoredContentPublishCommand(
                    tenantContext.TenantId,
                    contentId,
                    versionId,
                    actorUserId,
                    request.Environment,
                    request.Channel,
                    request.RequestedAtUtc ?? DateTimeOffset.UtcNow),
                cancellationToken);

            return result.Status switch
            {
                AuthoredContentPublishStatus.Succeeded => await PersistPublishDecisionAsync(
                    publishingHistoryRepository,
                    deliveryBundleRepository,
                    result,
                    cancellationToken),
                AuthoredContentPublishStatus.NotFound => Results.NotFound(PublishFailed("not_found", result.Issues)),
                AuthoredContentPublishStatus.InvalidRequest => Results.BadRequest(PublishFailed("invalid_publish_command", result.Issues)),
                _ => Results.BadRequest(PublishFailed("publish_validation_failed", result.Issues))
            };
        }
        catch (TenantAccessDeniedException)
        {
            return Results.NotFound(PublishFailed("not_found", [Issue("authored_content_not_found", "content", "Authored content was not found.")]));
        }
        catch
        {
            return Results.Problem(
                title: "Publish command failed.",
                detail: "The requested publish command could not be completed.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> PersistPublishDecisionAsync(
        IAuthoredContentPublishingHistoryRepository publishingHistoryRepository,
        IDeliveryBundleRepository deliveryBundleRepository,
        AuthoredContentPublishResult result,
        CancellationToken cancellationToken)
    {
        if (result.Bundle is null)
        {
            return Results.Problem(
                title: "Publish command failed.",
                detail: "The requested publish command could not be completed.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var storeResult = await deliveryBundleRepository.StoreAsync(result.Bundle, cancellationToken);
        if (storeResult.Status != DeliveryBundleLookupStatus.Found)
        {
            return Results.Problem(
                title: "Publish command failed.",
                detail: "The requested publish command could not be completed.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        if (result.AuditRecord is not null)
        {
            await publishingHistoryRepository.AddAsync(result.AuditRecord, cancellationToken);
        }

        return Results.Ok(new PublishAuthoredContentResponse(
            true,
            "succeeded",
            result.Bundle is null ? null : ToPublishBundleMetadataResponse(result.Bundle),
            ToPublishingAuditResponse(result.AuditRecord),
            []));
    }

    private static Guid ResolveActorUserId(
        HttpContext httpContext,
        IAdoptionTenantContext tenantContext)
    {
        var userMappingService = httpContext.RequestServices.GetRequiredService<IAuthenticatedUserMappingService>();
        var userMapping = userMappingService.MapUser(tenantContext.TenantId, httpContext.User);

        return userMapping.User?.Id ?? Guid.Empty;
    }

    private static AuthoredContentResponse ToResponse(
        AuthoredContentItem content,
        AuthoredContentReadSummaryResponse? summary = null)
    {
        return new AuthoredContentResponse(
            content.Id,
            content.TenantId,
            content.ApplicationId,
            content.ContentType,
            content.ContentKey,
            content.Title,
            content.Versions.Select(ToVersionResponse).ToArray(),
            summary);
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

    private static PublishBundleMetadataResponse ToPublishBundleMetadataResponse(DeliveryBundle bundle)
    {
        return new PublishBundleMetadataResponse(
            bundle.Content.BundleId,
            bundle.TenantId,
            bundle.ApplicationId,
            bundle.Environment,
            bundle.Channel,
            bundle.Content.Version,
            bundle.Content.GeneratedAtUtc,
            bundle.Content.Items.Count);
    }

    private static PublishingAuditResponse? ToPublishingAuditResponse(AuthoredContentPublishingAuditRecord? audit)
    {
        return audit is null
            ? null
            : new PublishingAuditResponse(
                audit.TenantId,
                audit.ContentId,
                audit.VersionId,
                audit.ActorUserId,
                audit.Environment,
                audit.Channel,
                audit.Result,
                audit.OccurredAtUtc);
    }

    private static AuthoredContentReadSummaryResponse BuildReadSummary(
        Guid tenantId,
        Guid contentId,
        IReadOnlyCollection<AuthoredContentLifecycleAuditRecord> lifecycleHistory,
        IReadOnlyCollection<AuthoredContentPublishingAuditRecord> publishingHistory)
    {
        var lifecycleRecords = lifecycleHistory
            .Where(record => record.TenantId == tenantId && record.ContentId == contentId)
            .ToArray();
        var publishingRecords = publishingHistory
            .Where(record => record.TenantId == tenantId && record.ContentId == contentId)
            .ToArray();
        var latestLifecycle = lifecycleRecords
            .OrderByDescending(record => record.OccurredAtUtc)
            .FirstOrDefault();
        var latestPublish = publishingRecords
            .OrderByDescending(record => record.OccurredAtUtc)
            .FirstOrDefault();

        return new AuthoredContentReadSummaryResponse(
            lifecycleRecords.Length,
            publishingRecords.Length,
            ResolveLatestSafeActivity(latestLifecycle, latestPublish),
            ResolveLatestActivityAtUtc(latestLifecycle, latestPublish),
            latestPublish is null ? null : ToLatestPublishSummary(latestPublish));
    }

    private static string ResolveLatestSafeActivity(
        AuthoredContentLifecycleAuditRecord? lifecycle,
        AuthoredContentPublishingAuditRecord? publish)
    {
        if (publish is not null &&
            (lifecycle is null || publish.OccurredAtUtc >= lifecycle.OccurredAtUtc))
        {
            return "Published to runtime delivery";
        }

        if (lifecycle is null)
        {
            return "No lifecycle or publishing history available.";
        }

        return lifecycle.LifecycleAction switch
        {
            "RequestReview" => "Review requested",
            "Approve" => "Approved for publishing",
            "Reject" => "Returned to draft",
            _ => "Lifecycle decision recorded"
        };
    }

    private static DateTimeOffset? ResolveLatestActivityAtUtc(
        AuthoredContentLifecycleAuditRecord? lifecycle,
        AuthoredContentPublishingAuditRecord? publish)
    {
        return (lifecycle, publish) switch
        {
            (null, null) => null,
            ({ } lifecycleRecord, null) => lifecycleRecord.OccurredAtUtc,
            (null, { } publishRecord) => publishRecord.OccurredAtUtc,
            ({ } lifecycleRecord, { } publishRecord) => publishRecord.OccurredAtUtc >= lifecycleRecord.OccurredAtUtc
                ? publishRecord.OccurredAtUtc
                : lifecycleRecord.OccurredAtUtc
        };
    }

    private static AuthoredContentLatestPublishSummaryResponse ToLatestPublishSummary(
        AuthoredContentPublishingAuditRecord audit)
    {
        return new AuthoredContentLatestPublishSummaryResponse(
            ToSafePublishStatus(audit.Result),
            ToSafeStructuralValue(audit.Environment),
            audit.Channel,
            audit.OccurredAtUtc);
    }

    private static string ToSafePublishStatus(string status)
    {
        return string.Equals(status, "Succeeded", StringComparison.Ordinal)
            ? "Succeeded"
            : "Recorded";
    }

    private static string ToSafeStructuralValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length is 0 or > 64)
        {
            return "unknown";
        }

        return trimmed.All(static character =>
            char.IsAsciiLetterOrDigit(character) ||
            character is '-' or '_' or '.')
            ? trimmed
            : "unknown";
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

    private static PublishAuthoredContentResponse PublishFailed(
        string status,
        IReadOnlyCollection<AuthoredContentValidationIssue> issues)
    {
        return new PublishAuthoredContentResponse(
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
