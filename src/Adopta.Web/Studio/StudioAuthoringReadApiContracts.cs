using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public sealed record StudioAuthoringContentApiResponse(
    Guid Id,
    Guid TenantId,
    Guid ApplicationId,
    StudioAuthoringContentTypeApiResponse? ContentType,
    string ContentKey,
    string Title,
    IReadOnlyCollection<StudioAuthoringContentVersionApiResponse> Versions,
    StudioAuthoringContentReadSummaryApiResponse? Summary);

public sealed record StudioAuthoringContentVersionApiResponse(
    Guid Id,
    string Version,
    StudioAuthoringLifecycleStateApiResponse LifecycleState,
    DateTimeOffset CreatedAtUtc);

public sealed record StudioAuthoringContentListApiResponse(
    IReadOnlyCollection<StudioAuthoringContentApiResponse> Items);

public sealed record StudioAuthoringContentReadSummaryApiResponse(
    int LifecycleEventCount,
    int PublishingEventCount,
    string LatestSafeActivity,
    DateTimeOffset? LatestActivityAtUtc,
    StudioAuthoringLatestPublishSummaryApiResponse? LatestPublish);

public sealed record StudioAuthoringLatestPublishSummaryApiResponse(
    string Status,
    string Environment,
    DeliveryChannel Channel,
    DateTimeOffset OccurredAtUtc);

public enum StudioAuthoringContentTypeApiResponse
{
    Tooltip = 0,
    Callout = 1,
    Checklist = 2,
    Walkthrough = 3
}

public enum StudioAuthoringLifecycleStateApiResponse
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Published = 3,
    Archived = 4
}
