namespace Adopta.Web.Studio;

public sealed record StudioAuthoringContentApiResponse(
    Guid Id,
    Guid TenantId,
    Guid ApplicationId,
    string ContentKey,
    string Title,
    IReadOnlyCollection<StudioAuthoringContentVersionApiResponse> Versions);

public sealed record StudioAuthoringContentVersionApiResponse(
    Guid Id,
    string Version,
    StudioAuthoringLifecycleStateApiResponse LifecycleState,
    DateTimeOffset CreatedAtUtc);

public sealed record StudioAuthoringContentListApiResponse(
    IReadOnlyCollection<StudioAuthoringContentApiResponse> Items);

public enum StudioAuthoringLifecycleStateApiResponse
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Published = 3,
    Archived = 4
}
