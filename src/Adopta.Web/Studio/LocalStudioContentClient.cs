using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public sealed class LocalStudioContentClient : IStudioContentClient
{
    private readonly List<StudioContentListItem> items = StudioContentFoundationData.Loaded().Items.ToList();

    public Task<StudioContentClientResult<StudioContentPageModel>> ListAsync(
        StudioContentListRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var model = StudioContentFoundationData.Loaded() with
        {
            Items = items.ToArray(),
            SelectedContentId = items.FirstOrDefault()?.Id
        };

        if (request.ApplicationId is { } applicationId)
        {
            var filteredItems = items
                .Where(item => item.ApplicationId == applicationId)
                .ToArray();

            model = filteredItems.Length == 0
                ? StudioContentFoundationData.Empty()
                : model with
                {
                    Items = filteredItems,
                    SelectedContentId = filteredItems[0].Id
                };
        }

        return Task.FromResult(StudioContentClientResult<StudioContentPageModel>.Success(model));
    }

    public Task<StudioContentClientResult<StudioContentListItem>> GetByIdAsync(
        StudioContentGetByIdRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ContentId == Guid.Empty)
        {
            return Task.FromResult(StudioContentClientResult<StudioContentListItem>.InvalidResponse());
        }

        var item = items
            .FirstOrDefault(content => content.Id == request.ContentId);

        return Task.FromResult(item is null
            ? StudioContentClientResult<StudioContentListItem>.NotFound()
            : StudioContentClientResult<StudioContentListItem>.Success(item));
    }

    public Task<StudioContentClientResult<StudioContentEditorModel>> CreateDraftAsync(
        StudioContentCreateDraftRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var editor = StudioContentEditorModel.FromCreateRequest(request);
        var validation = editor.Validate();
        if (!validation.Succeeded)
        {
            editor.ApplyValidationResult(validation);
            return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.ValidationError(editor));
        }

        editor.ContentId = Guid.NewGuid();
        editor.LifecycleState = StudioContentLifecycleState.Draft;
        editor.MarkSaved();
        UpsertItem(CreateItemFromEditor(editor, null));

        return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.Success(
            editor,
            "Draft metadata saved locally."));
    }

    public Task<StudioContentClientResult<StudioContentEditorModel>> UpdateDraftAsync(
        StudioContentUpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ContentId == Guid.Empty)
        {
            return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.InvalidResponse());
        }

        var existing = items
            .FirstOrDefault(content => content.Id == request.ContentId);

        if (existing is null)
        {
            return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.NotFound());
        }

        if (existing.LifecycleState != StudioContentLifecycleState.Draft)
        {
            return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.Forbidden());
        }

        var editor = StudioContentEditorModel.FromUpdateRequest(request, existing.LifecycleState);
        var validation = editor.Validate();
        if (!validation.Succeeded)
        {
            editor.ApplyValidationResult(validation);
            return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.ValidationError(editor));
        }

        editor.MarkSaved();
        UpsertItem(CreateItemFromEditor(editor, existing));

        return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.Success(
            editor,
            "Draft metadata saved locally."));
    }

    public Task<StudioContentClientResult<StudioWorkflowActionModel>> RequestReviewAsync(
        StudioWorkflowActionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteWorkflowActionAsync(
            request,
            StudioWorkflowActionKind.RequestReview,
            cancellationToken);

    public Task<StudioContentClientResult<StudioWorkflowActionModel>> ApproveAsync(
        StudioWorkflowActionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteWorkflowActionAsync(
            request,
            StudioWorkflowActionKind.Approve,
            cancellationToken);

    public Task<StudioContentClientResult<StudioWorkflowActionModel>> RejectAsync(
        StudioWorkflowActionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteWorkflowActionAsync(
            request,
            StudioWorkflowActionKind.Reject,
            cancellationToken);

    public Task<StudioContentClientResult<StudioPublishActionModel>> PublishAsync(
        StudioPublishActionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ContentId == Guid.Empty || request.VersionId == Guid.Empty)
        {
            return Task.FromResult(StudioContentClientResult<StudioPublishActionModel>.InvalidResponse());
        }

        var content = items
            .FirstOrDefault(item => item.Id == request.ContentId);

        if (content?.CurrentVersion is null || content.CurrentVersion.Id != request.VersionId)
        {
            return Task.FromResult(StudioContentClientResult<StudioPublishActionModel>.NotFound());
        }

        var publish = StudioPublishActionModel.FromContent(content);
        publish.Environment = request.Environment ?? string.Empty;
        publish.Channel = request.Channel;

        var validation = publish.Validate();
        if (!validation.Succeeded)
        {
            publish.ApplyValidationResult(validation);
            return Task.FromResult(StudioContentClientResult<StudioPublishActionModel>.ValidationError(publish));
        }

        publish.MarkPublished();
        UpsertItem(UpdateItemLifecycle(
            content,
            publish.VersionId,
            StudioContentLifecycleState.Published,
            publish.SafeMessage,
            incrementPublishingEvents: true));

        return Task.FromResult(StudioContentClientResult<StudioPublishActionModel>.Success(
            publish,
            "Publish validation completed locally."));
    }

    private Task<StudioContentClientResult<StudioWorkflowActionModel>> ExecuteWorkflowActionAsync(
        StudioWorkflowActionRequest request,
        StudioWorkflowActionKind action,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ContentId == Guid.Empty || request.VersionId == Guid.Empty)
        {
            return Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.InvalidResponse());
        }

        var content = items
            .FirstOrDefault(item => item.Id == request.ContentId);

        if (content?.CurrentVersion is null || content.CurrentVersion.Id != request.VersionId)
        {
            return Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.NotFound());
        }

        var workflow = StudioWorkflowActionModel.FromContent(content);
        var validation = workflow.Validate(action);
        if (!validation.Succeeded)
        {
            workflow.ApplyValidationResult(validation);
            return Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.ValidationError(workflow));
        }

        workflow.MarkSucceeded(action);
        UpsertItem(UpdateItemLifecycle(
            content,
            workflow.VersionId,
            workflow.LifecycleState,
            workflow.SafeMessage));

        return Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.Success(
            workflow,
            "Review workflow action completed locally."));
    }

    private void UpsertItem(StudioContentListItem item)
    {
        var existingIndex = items.FindIndex(existing => existing.Id == item.Id);
        if (existingIndex < 0)
        {
            items.Add(item);
            return;
        }

        items[existingIndex] = item;
    }

    private static StudioContentListItem CreateItemFromEditor(
        StudioContentEditorModel editor,
        StudioContentListItem? existing)
    {
        var savedAtUtc = DateTimeOffset.UtcNow;
        var contentType = editor.ContentType ?? RuntimeContentType.Tooltip;
        var contentId = editor.ContentId ?? Guid.NewGuid();

        return new StudioContentListItem(
            contentId,
            editor.ApplicationId,
            editor.ContentKey.Trim(),
            editor.Title.Trim(),
            contentType,
            StudioContentLifecycleState.Draft,
            [
                new StudioContentVersionSummary(
                    existing?.CurrentVersion?.Id ?? Guid.NewGuid(),
                    editor.Version,
                    StudioContentLifecycleState.Draft,
                    existing?.CurrentVersion?.CreatedAtUtc ?? savedAtUtc)
            ],
            new StudioContentHistorySummary(
                existing?.History.LifecycleEventCount ?? 1,
                existing?.History.PublishingEventCount ?? 0,
                "Draft metadata saved locally",
                savedAtUtc));
    }

    private static StudioContentListItem UpdateItemLifecycle(
        StudioContentListItem item,
        Guid versionId,
        StudioContentLifecycleState lifecycleState,
        string latestSafeActivity,
        bool incrementPublishingEvents = false)
    {
        var occurredAtUtc = DateTimeOffset.UtcNow;
        var versions = item.Versions
            .Select(version => version.Id == versionId
                ? version with
                {
                    LifecycleState = lifecycleState
                }
                : version)
            .ToArray();

        return item with
        {
            LifecycleState = lifecycleState,
            Versions = versions,
            History = item.History with
            {
                LifecycleEventCount = incrementPublishingEvents
                    ? item.History.LifecycleEventCount
                    : item.History.LifecycleEventCount + 1,
                PublishingEventCount = incrementPublishingEvents
                    ? item.History.PublishingEventCount + 1
                    : item.History.PublishingEventCount,
                LatestSafeActivity = latestSafeActivity,
                LatestActivityAtUtc = occurredAtUtc
            }
        };
    }
}
