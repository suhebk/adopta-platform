namespace Adopta.Web.Studio;

public sealed class LocalStudioContentClient : IStudioContentClient
{
    public Task<StudioContentClientResult<StudioContentPageModel>> ListAsync(
        StudioContentListRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var model = StudioContentFoundationData.Loaded();
        if (request.ApplicationId is { } applicationId)
        {
            var filteredItems = model.Items
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

        var item = StudioContentFoundationData.Loaded()
            .Items
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

        var existing = StudioContentFoundationData.Loaded()
            .Items
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

        return Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.Success(
            editor,
            "Draft metadata saved locally."));
    }
}
