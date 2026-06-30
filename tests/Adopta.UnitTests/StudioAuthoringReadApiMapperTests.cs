using Adopta.Application.Runtime;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioAuthoringReadApiMapperTests
{
    [Fact]
    public void Mapper_drops_tenant_identifiers_from_studio_read_model()
    {
        var response = BuildContentResponse();

        var item = StudioAuthoringReadApiMapper.MapItem(response);

        Assert.Equal(response.Id, item.Id);
        Assert.Equal(response.ApplicationId, item.ApplicationId);
        Assert.DoesNotContain(
            item.GetType().GetProperties(),
            property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(StudioContentPageModel).GetProperties(),
            property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Mapper_marks_missing_content_type_as_unavailable()
    {
        var response = BuildContentResponse();

        var item = StudioAuthoringReadApiMapper.MapItem(response);

        Assert.False(item.HasKnownContentType);
        Assert.Equal(RuntimeContentType.Tooltip, item.ContentType);
        Assert.Equal("Limited authoring API metadata loaded.", item.History.LatestSafeActivity);
    }

    [Fact]
    public void Mapper_handles_limited_audit_and_history_metadata_safely()
    {
        var response = BuildContentResponse();

        var item = StudioAuthoringReadApiMapper.MapItem(response);

        Assert.Equal(2, item.History.LifecycleEventCount);
        Assert.Equal(0, item.History.PublishingEventCount);
        Assert.Equal(response.Versions.Max(version => version.CreatedAtUtc), item.History.LatestActivityAtUtc);
        Assert.DoesNotContain("tenant", item.History.LatestSafeActivity, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", item.History.LatestSafeActivity, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Mapper_maps_list_and_filters_by_application()
    {
        var applicationA = Guid.NewGuid();
        var applicationB = Guid.NewGuid();
        var response = new StudioAuthoringContentListApiResponse(
            [
                BuildContentResponse(applicationId: applicationA, title: "Welcome"),
                BuildContentResponse(applicationId: applicationB, title: "Billing")
            ]);

        var page = StudioAuthoringReadApiMapper.MapList(response, applicationA);

        Assert.Equal(StudioContentPageState.Loaded, page.State);
        var item = Assert.Single(page.Items);
        Assert.Equal(applicationA, item.ApplicationId);
        Assert.Equal(item.Id, page.SelectedContentId);
    }

    [Fact]
    public void Mapper_maps_empty_list_to_safe_empty_state()
    {
        var page = StudioAuthoringReadApiMapper.MapList(
            new StudioAuthoringContentListApiResponse([]));

        Assert.Equal(StudioContentPageState.Empty, page.State);
        Assert.Empty(page.Items);
        Assert.DoesNotContain("tenant", page.SafeMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static StudioAuthoringContentApiResponse BuildContentResponse(
        Guid? applicationId = null,
        string title = "Welcome tooltip")
    {
        var latest = DateTimeOffset.Parse("2026-06-30T10:00:00Z");
        var earlier = DateTimeOffset.Parse("2026-06-29T10:00:00Z");

        return new StudioAuthoringContentApiResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            applicationId ?? Guid.NewGuid(),
            "welcome.tooltip",
            title,
            [
                new StudioAuthoringContentVersionApiResponse(
                    Guid.NewGuid(),
                    "0.1.0",
                    StudioAuthoringLifecycleStateApiResponse.Draft,
                    earlier),
                new StudioAuthoringContentVersionApiResponse(
                    Guid.NewGuid(),
                    "1.0.0",
                    StudioAuthoringLifecycleStateApiResponse.Approved,
                    latest)
            ]);
    }
}
