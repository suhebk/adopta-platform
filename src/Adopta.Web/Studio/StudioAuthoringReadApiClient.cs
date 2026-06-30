using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Adopta.Web.Studio;

public sealed class StudioAuthoringReadApiClient : IStudioContentClient
{
    private readonly HttpClient httpClient;

    public StudioAuthoringReadApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<StudioContentClientResult<StudioContentPageModel>> ListAsync(
        StudioContentListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync("/authoring/content", cancellationToken);
            if (!IsSuccess(response.StatusCode))
            {
                return MapFailure<StudioContentPageModel>(response.StatusCode);
            }

            var body = await response.Content.ReadFromJsonAsync<StudioAuthoringContentListApiResponse>(
                cancellationToken);
            if (body is null)
            {
                return StudioContentClientResult<StudioContentPageModel>.InvalidResponse();
            }

            return StudioContentClientResult<StudioContentPageModel>.Success(
                StudioAuthoringReadApiMapper.MapList(body, request.ApplicationId),
                "Authored content loaded from the authoring API.");
        }
        catch (JsonException)
        {
            return StudioContentClientResult<StudioContentPageModel>.InvalidResponse();
        }
        catch (NotSupportedException)
        {
            return StudioContentClientResult<StudioContentPageModel>.InvalidResponse();
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return StudioContentClientResult<StudioContentPageModel>.Unavailable();
        }
        catch
        {
            return StudioContentClientResult<StudioContentPageModel>.UnexpectedError();
        }
    }

    public async Task<StudioContentClientResult<StudioContentListItem>> GetByIdAsync(
        StudioContentGetByIdRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentId == Guid.Empty)
        {
            return StudioContentClientResult<StudioContentListItem>.InvalidResponse();
        }

        try
        {
            using var response = await httpClient.GetAsync(
                $"/authoring/content/{request.ContentId:D}",
                cancellationToken);
            if (!IsSuccess(response.StatusCode))
            {
                return MapFailure<StudioContentListItem>(response.StatusCode);
            }

            var body = await response.Content.ReadFromJsonAsync<StudioAuthoringContentApiResponse>(
                cancellationToken);
            if (body is null)
            {
                return StudioContentClientResult<StudioContentListItem>.InvalidResponse();
            }

            return StudioContentClientResult<StudioContentListItem>.Success(
                StudioAuthoringReadApiMapper.MapItem(body),
                "Authored content loaded from the authoring API.");
        }
        catch (JsonException)
        {
            return StudioContentClientResult<StudioContentListItem>.InvalidResponse();
        }
        catch (NotSupportedException)
        {
            return StudioContentClientResult<StudioContentListItem>.InvalidResponse();
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return StudioContentClientResult<StudioContentListItem>.Unavailable();
        }
        catch
        {
            return StudioContentClientResult<StudioContentListItem>.UnexpectedError();
        }
    }

    public Task<StudioContentClientResult<StudioContentEditorModel>> CreateDraftAsync(
        StudioContentCreateDraftRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.Unavailable());

    public Task<StudioContentClientResult<StudioContentEditorModel>> UpdateDraftAsync(
        StudioContentUpdateDraftRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(StudioContentClientResult<StudioContentEditorModel>.Unavailable());

    public Task<StudioContentClientResult<StudioWorkflowActionModel>> RequestReviewAsync(
        StudioWorkflowActionRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.Unavailable());

    public Task<StudioContentClientResult<StudioWorkflowActionModel>> ApproveAsync(
        StudioWorkflowActionRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.Unavailable());

    public Task<StudioContentClientResult<StudioWorkflowActionModel>> RejectAsync(
        StudioWorkflowActionRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(StudioContentClientResult<StudioWorkflowActionModel>.Unavailable());

    public Task<StudioContentClientResult<StudioPublishActionModel>> PublishAsync(
        StudioPublishActionRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(StudioContentClientResult<StudioPublishActionModel>.Unavailable());

    private static bool IsSuccess(HttpStatusCode statusCode) =>
        statusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices;

    private static StudioContentClientResult<T> MapFailure<T>(HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.Unauthorized => StudioContentClientResult<T>.Unauthorized(),
            HttpStatusCode.Forbidden => StudioContentClientResult<T>.Forbidden(),
            HttpStatusCode.NotFound => StudioContentClientResult<T>.NotFound(),
            HttpStatusCode.BadRequest => StudioContentClientResult<T>.InvalidResponse(),
            HttpStatusCode.RequestTimeout => StudioContentClientResult<T>.Unavailable(),
            >= HttpStatusCode.InternalServerError => StudioContentClientResult<T>.Unavailable(),
            _ => StudioContentClientResult<T>.UnexpectedError()
        };

    private static bool IsTransportFailure(Exception exception) =>
        exception is HttpRequestException
            or TaskCanceledException
            or TimeoutException;
}
