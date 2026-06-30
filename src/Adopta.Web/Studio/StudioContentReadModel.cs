using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public enum StudioContentLifecycleState
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Published = 3,
    Archived = 4
}

public enum StudioContentPageState
{
    Loading = 0,
    Empty = 1,
    Error = 2,
    NotAuthorized = 3,
    Loaded = 4
}

public sealed record StudioContentPageModel(
    StudioContentPageState State,
    IReadOnlyCollection<StudioContentListItem> Items,
    Guid? SelectedContentId,
    string SafeMessage)
{
    public StudioContentListItem? SelectedContent =>
        Items.FirstOrDefault(item => item.Id == SelectedContentId) ?? Items.FirstOrDefault();

    public int CountByState(StudioContentLifecycleState state) =>
        Items.Count(item => item.LifecycleState == state);
}

public sealed record StudioContentListItem(
    Guid Id,
    Guid ApplicationId,
    string ContentKey,
    string Title,
    RuntimeContentType ContentType,
    StudioContentLifecycleState LifecycleState,
    IReadOnlyCollection<StudioContentVersionSummary> Versions,
    StudioContentHistorySummary History)
{
    public bool HasKnownContentType { get; init; } = true;

    public StudioContentVersionSummary? CurrentVersion => Versions.FirstOrDefault();
}

public sealed record StudioContentVersionSummary(
    Guid Id,
    string Version,
    StudioContentLifecycleState LifecycleState,
    DateTimeOffset CreatedAtUtc);

public sealed record StudioContentHistorySummary(
    int LifecycleEventCount,
    int PublishingEventCount,
    string LatestSafeActivity,
    DateTimeOffset? LatestActivityAtUtc);
