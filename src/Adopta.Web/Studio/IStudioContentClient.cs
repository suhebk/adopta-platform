namespace Adopta.Web.Studio;

public interface IStudioContentClient
{
    Task<StudioContentClientResult<StudioContentPageModel>> ListAsync(
        StudioContentListRequest request,
        CancellationToken cancellationToken);

    Task<StudioContentClientResult<StudioContentListItem>> GetByIdAsync(
        StudioContentGetByIdRequest request,
        CancellationToken cancellationToken);

    Task<StudioContentClientResult<StudioContentEditorModel>> CreateDraftAsync(
        StudioContentCreateDraftRequest request,
        CancellationToken cancellationToken);

    Task<StudioContentClientResult<StudioContentEditorModel>> UpdateDraftAsync(
        StudioContentUpdateDraftRequest request,
        CancellationToken cancellationToken);
}
