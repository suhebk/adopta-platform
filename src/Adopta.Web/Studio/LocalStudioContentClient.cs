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
}
