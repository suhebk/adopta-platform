using Adopta.Application.Identity;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioNavigationTests
{
    [Fact]
    public void Studio_route_paths_are_unique()
    {
        var paths = StudioNavigation.Items.Select(item => item.RoutePath).ToArray();

        Assert.Equal(paths.Length, paths.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Expected_shell_routes_exist()
    {
        var paths = StudioNavigation.Items.Select(item => item.RoutePath).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("/studio", paths);
        Assert.Contains("/studio/content", paths);
        Assert.Contains("/studio/review", paths);
        Assert.Contains("/studio/publishing", paths);
        Assert.Contains("/studio/governance", paths);
    }

    [Fact]
    public void Studio_navigation_permissions_use_existing_catalog()
    {
        Assert.All(StudioNavigation.Items, item =>
        {
            Assert.Contains(item.RequiredPermissionKey, AdoptaPermissionKeys.All);
        });
    }

    [Fact]
    public void Studio_content_route_requires_authoring_read_permission()
    {
        var item = Assert.Single(
            StudioNavigation.Items,
            item => item.RoutePath == "/studio/content");

        Assert.Equal(AdoptaPermissionKeys.AuthoringRead, item.RequiredPermissionKey);
    }

    [Fact]
    public void Studio_navigation_metadata_is_complete()
    {
        Assert.All(StudioNavigation.Items, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.RoutePath));
            Assert.False(string.IsNullOrWhiteSpace(item.Label));
            Assert.False(string.IsNullOrWhiteSpace(item.Group));
            Assert.False(string.IsNullOrWhiteSpace(item.Description));
            Assert.Equal(StudioNavigation.FoundationShellStatus, item.ShellStatus);
        });
    }
}
