namespace Adopta.Web.Studio;

public static class StudioContentFoundationData
{
    public const string LocalDataNotice =
        "Local foundation data. Authenticated tenant-aware API integration is deferred.";

    public static StudioContentPageModel Loading() =>
        new(StudioContentPageState.Loading, [], null, "Loading authored content.");

    public static StudioContentPageModel Empty() =>
        new(StudioContentPageState.Empty, [], null, "No authored content is available.");

    public static StudioContentPageModel Error() =>
        new(StudioContentPageState.Error, [], null, "Authored content could not be loaded.");

    public static StudioContentPageModel NotAuthorized() =>
        new(StudioContentPageState.NotAuthorized, [], null, "You do not have access to authored content.");

    public static StudioContentPageModel Loaded()
    {
        var items = new[]
        {
            CreateItem(
                "10000000-0000-0000-0000-000000000001",
                "20000000-0000-0000-0000-000000000001",
                "30000000-0000-0000-0000-000000000001",
                "welcome.tooltip",
                "Welcome tooltip",
                StudioContentLifecycleState.Draft,
                "0.1.0",
                1,
                0,
                "Draft version created",
                "2026-06-20T09:00:00Z"),
            CreateItem(
                "10000000-0000-0000-0000-000000000002",
                "20000000-0000-0000-0000-000000000001",
                "30000000-0000-0000-0000-000000000002",
                "setup.review.walkthrough",
                "Setup walkthrough",
                StudioContentLifecycleState.InReview,
                "0.3.0",
                3,
                0,
                "Review requested",
                "2026-06-21T11:30:00Z"),
            CreateItem(
                "10000000-0000-0000-0000-000000000003",
                "20000000-0000-0000-0000-000000000002",
                "30000000-0000-0000-0000-000000000003",
                "release.checklist",
                "Release readiness checklist",
                StudioContentLifecycleState.Approved,
                "1.0.0",
                4,
                0,
                "Approved for publishing",
                "2026-06-22T14:00:00Z"),
            CreateItem(
                "10000000-0000-0000-0000-000000000004",
                "20000000-0000-0000-0000-000000000002",
                "30000000-0000-0000-0000-000000000004",
                "navigation.callout",
                "Navigation callout",
                StudioContentLifecycleState.Published,
                "1.2.0",
                6,
                2,
                "Published to runtime delivery",
                "2026-06-23T16:45:00Z"),
            CreateItem(
                "10000000-0000-0000-0000-000000000005",
                "20000000-0000-0000-0000-000000000003",
                "30000000-0000-0000-0000-000000000005",
                "legacy.banner",
                "Legacy banner",
                StudioContentLifecycleState.Archived,
                "0.9.0",
                5,
                1,
                "Archived after replacement",
                "2026-06-24T10:15:00Z")
        };

        return new StudioContentPageModel(
            StudioContentPageState.Loaded,
            items,
            items[0].Id,
            LocalDataNotice);
    }

    private static StudioContentListItem CreateItem(
        string id,
        string applicationId,
        string versionId,
        string contentKey,
        string title,
        StudioContentLifecycleState lifecycleState,
        string version,
        int lifecycleEventCount,
        int publishingEventCount,
        string latestSafeActivity,
        string latestActivityAtUtc)
    {
        var createdAtUtc = DateTimeOffset.Parse(
            latestActivityAtUtc,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal);

        return new StudioContentListItem(
            Guid.Parse(id),
            Guid.Parse(applicationId),
            contentKey,
            title,
            lifecycleState,
            [
                new StudioContentVersionSummary(
                    Guid.Parse(versionId),
                    version,
                    lifecycleState,
                    createdAtUtc)
            ],
            new StudioContentHistorySummary(
                lifecycleEventCount,
                publishingEventCount,
                latestSafeActivity,
                createdAtUtc));
    }
}
