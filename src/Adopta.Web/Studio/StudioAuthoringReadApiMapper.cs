using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public static class StudioAuthoringReadApiMapper
{
    public static StudioContentPageModel MapList(
        StudioAuthoringContentListApiResponse response,
        Guid? applicationId = null)
    {
        var items = response.Items
            .Select(MapItem)
            .Where(item => applicationId is null || item.ApplicationId == applicationId.Value)
            .OrderBy(item => item.Title, StringComparer.Ordinal)
            .ToArray();

        return items.Length == 0
            ? StudioContentFoundationData.Empty() with
            {
                SafeMessage = "No authored content is available from the authoring API."
            }
            : new StudioContentPageModel(
                StudioContentPageState.Loaded,
                items,
                items[0].Id,
                "Authored content loaded from the authoring API.");
    }

    public static StudioContentListItem MapItem(
        StudioAuthoringContentApiResponse response)
    {
        var versions = response.Versions
            .Select(MapVersion)
            .OrderByDescending(version => version.CreatedAtUtc)
            .ToArray();
        var lifecycleState = versions.FirstOrDefault()?.LifecycleState ?? StudioContentLifecycleState.Draft;
        var latestActivityAtUtc = versions.FirstOrDefault()?.CreatedAtUtc;

        return new StudioContentListItem(
            response.Id,
            response.ApplicationId,
            response.ContentKey,
            response.Title,
            RuntimeContentType.Tooltip,
            lifecycleState,
            versions,
            new StudioContentHistorySummary(
                versions.Length,
                0,
                "Limited authoring API metadata loaded.",
                latestActivityAtUtc))
        {
            HasKnownContentType = false
        };
    }

    private static StudioContentVersionSummary MapVersion(
        StudioAuthoringContentVersionApiResponse version)
    {
        return new StudioContentVersionSummary(
            version.Id,
            version.Version,
            MapLifecycleState(version.LifecycleState),
            version.CreatedAtUtc);
    }

    private static StudioContentLifecycleState MapLifecycleState(
        StudioAuthoringLifecycleStateApiResponse lifecycleState) =>
        lifecycleState switch
        {
            StudioAuthoringLifecycleStateApiResponse.InReview => StudioContentLifecycleState.InReview,
            StudioAuthoringLifecycleStateApiResponse.Approved => StudioContentLifecycleState.Approved,
            StudioAuthoringLifecycleStateApiResponse.Published => StudioContentLifecycleState.Published,
            StudioAuthoringLifecycleStateApiResponse.Archived => StudioContentLifecycleState.Archived,
            _ => StudioContentLifecycleState.Draft
        };
}
