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
        Assert.Contains("@rendermode InteractiveServer", markup, StringComparison.Ordinal);
        Assert.Contains("@inject IStudioContentClient StudioContentClient", markup, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"studio-content-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("<table class=\"studio-content__table\">", markup, StringComparison.Ordinal);
        Assert.Contains("<caption>Authored content metadata</caption>", markup, StringComparison.Ordinal);
        Assert.Contains("Content inventory", markup, StringComparison.Ordinal);
        Assert.Contains("Version metadata", markup, StringComparison.Ordinal);
        Assert.Contains("Audit and history summary", markup, StringComparison.Ordinal);
        Assert.Contains("Review workflow", markup, StringComparison.Ordinal);
        Assert.Contains("Publish readiness", markup, StringComparison.Ordinal);
        Assert.Contains("Guidance metadata editor", markup, StringComparison.Ordinal);
        Assert.Contains("Not authorized", markup, StringComparison.Ordinal);
        Assert.Contains("Loading content", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_uses_client_boundary_for_loaded_data()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("StudioContentClient.ListAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioContentClient.CreateDraftAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioContentClient.UpdateDraftAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioContentClient.RequestReviewAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioContentClient.ApproveAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioContentClient.RejectAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioContentClient.PublishAsync", markup, StringComparison.Ordinal);
        Assert.Contains("new StudioContentListRequest()", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("StudioContentFoundationData.Loaded()", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("@inject LocalStudioContentClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("@inject StudioAuthoringReadApiClient", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_contains_accessible_metadata_editor_markup()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("<form class=\"studio-content__editor-form\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"guidance-editor-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("id=\"guidance-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("id=\"guidance-content-key\"", markup, StringComparison.Ordinal);
        Assert.Contains("id=\"guidance-content-type\"", markup, StringComparison.Ordinal);
        Assert.Contains("id=\"guidance-application\"", markup, StringComparison.Ordinal);
        Assert.Contains("Validation summary", markup, StringComparison.Ordinal);
        Assert.Contains("Save draft", markup, StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite\"", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_contains_accessible_workflow_markup()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("aria-labelledby=\"review-workflow-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("Workflow validation summary", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Review workflow actions\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Request review for selected draft\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Approve selected content\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Return selected content to draft\"", markup, StringComparison.Ordinal);
        Assert.Contains("role=\"status\" aria-live=\"polite\"", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_contains_accessible_publish_markup()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("aria-labelledby=\"publish-readiness-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("Publish validation summary", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Publish target\"", markup, StringComparison.Ordinal);
        Assert.Contains("id=\"publish-environment\"", markup, StringComparison.Ordinal);
        Assert.Contains("id=\"publish-channel\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Publish actions\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Publish approved content\"", markup, StringComparison.Ordinal);
        Assert.Contains("role=\"status\" aria-live=\"polite\"", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_guards_workflow_actions_by_lifecycle_state()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("IsWorkflowActionAvailable(StudioWorkflowActionKind.RequestReview)", markup, StringComparison.Ordinal);
        Assert.Contains("IsWorkflowActionAvailable(StudioWorkflowActionKind.Approve)", markup, StringComparison.Ordinal);
        Assert.Contains("IsWorkflowActionAvailable(StudioWorkflowActionKind.Reject)", markup, StringComparison.Ordinal);
        Assert.Contains("RunWorkflowActionAsync(StudioWorkflowActionKind.RequestReview)", markup, StringComparison.Ordinal);
        Assert.Contains("RunWorkflowActionAsync(StudioWorkflowActionKind.Approve)", markup, StringComparison.Ordinal);
        Assert.Contains("RunWorkflowActionAsync(StudioWorkflowActionKind.Reject)", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_content_page_guards_publish_action_by_lifecycle_state()
    {
        var markup = ReadRepositoryFile("src/Adopta.Web/Components/Pages/Studio/StudioContent.razor");

        Assert.Contains("StudioPublishActionModel.IsPublishAvailable", markup, StringComparison.Ordinal);
        Assert.Contains("@if (IsPublishActionAvailable)", markup, StringComparison.Ordinal);
        Assert.Contains("RunPublishAsync", markup, StringComparison.Ordinal);
        Assert.Contains("new StudioPublishActionRequest", markup, StringComparison.Ordinal);
        Assert.Contains("No publish action is available.", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("StudioWorkflowActionKind.Publish", markup, StringComparison.Ordinal);
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
