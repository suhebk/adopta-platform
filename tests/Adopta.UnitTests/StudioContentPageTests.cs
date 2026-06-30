using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioContentPageTests
{
    [Fact]
    public void Foundation_data_covers_all_lifecycle_states()
    {
        var model = StudioContentFoundationData.Loaded();
        var states = model.Items.Select(item => item.LifecycleState).ToHashSet();

        Assert.Equal(StudioContentPageState.Loaded, model.State);
        Assert.Contains(StudioContentLifecycleState.Draft, states);
        Assert.Contains(StudioContentLifecycleState.InReview, states);
        Assert.Contains(StudioContentLifecycleState.Approved, states);
        Assert.Contains(StudioContentLifecycleState.Published, states);
        Assert.Contains(StudioContentLifecycleState.Archived, states);
    }

    [Fact]
    public void Foundation_data_contains_safe_structural_metadata_only()
    {
        var model = StudioContentFoundationData.Loaded();
        var serialized = string.Join(
            "|",
            model.Items.Select(item => string.Join(
                "|",
                item.Id,
                item.ApplicationId,
                item.ContentKey,
                item.Title,
                item.LifecycleState,
                item.CurrentVersion?.Version,
                item.History.LatestSafeActivity)));

        Assert.DoesNotContain("token", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Page_model_exposes_safe_ui_states()
    {
        Assert.Equal(StudioContentPageState.Loading, StudioContentFoundationData.Loading().State);
        Assert.Equal(StudioContentPageState.Empty, StudioContentFoundationData.Empty().State);
        Assert.Equal(StudioContentPageState.Error, StudioContentFoundationData.Error().State);
        Assert.Equal(StudioContentPageState.NotAuthorized, StudioContentFoundationData.NotAuthorized().State);
        Assert.Equal(StudioContentPageState.Loaded, StudioContentFoundationData.Loaded().State);

        Assert.All(new[]
        {
            StudioContentFoundationData.Loading(),
            StudioContentFoundationData.Empty(),
            StudioContentFoundationData.Error(),
            StudioContentFoundationData.NotAuthorized(),
            StudioContentFoundationData.Loaded()
        }, model => Assert.False(string.IsNullOrWhiteSpace(model.SafeMessage)));
    }

    [Fact]
    public void Studio_content_page_contains_accessible_read_only_inventory_markup()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("@page \"/studio/content\"", markup, StringComparison.Ordinal);
        Assert.Contains("@inject IStudioContentClient StudioContentClient", markup, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"studio-content-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("<table class=\"studio-content__table\">", markup, StringComparison.Ordinal);
        Assert.Contains("<caption>Authored content metadata</caption>", markup, StringComparison.Ordinal);
        Assert.Contains("Content inventory", markup, StringComparison.Ordinal);
        Assert.Contains("Version metadata", markup, StringComparison.Ordinal);
        Assert.Contains("Audit and history summary", markup, StringComparison.Ordinal);
        Assert.Contains("Not authorized", markup, StringComparison.Ordinal);
        Assert.Contains("Loading content", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_uses_client_boundary_for_loaded_data()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("StudioContentClient.ListAsync", markup, StringComparison.Ordinal);
        Assert.Contains("new StudioContentListRequest()", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("StudioContentFoundationData.Loaded()", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_does_not_add_authoring_action_ui()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.DoesNotContain("Create content", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Edit content", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Request review", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Approve content", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Reject content", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Publish content", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<button", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<form", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<input", markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Studio_content_page_avoids_unsafe_output_patterns()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.DoesNotContain("MarkupString", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("innerHTML", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Secret", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HMRC", markup, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Adopta.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return File.ReadAllText(Path.Combine(directory.FullName, relativePath));
    }
}
