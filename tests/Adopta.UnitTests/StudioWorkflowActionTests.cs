using Adopta.Application.Identity;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioWorkflowActionTests
{
    [Fact]
    public void Workflow_model_exposes_allowed_actions_by_lifecycle_state()
    {
        Assert.Equal(
            [StudioWorkflowActionKind.RequestReview],
            StudioWorkflowActionModel.GetAvailableActions(StudioContentLifecycleState.Draft));

        Assert.Equal(
            [StudioWorkflowActionKind.Approve, StudioWorkflowActionKind.Reject],
            StudioWorkflowActionModel.GetAvailableActions(StudioContentLifecycleState.InReview));

        Assert.Empty(StudioWorkflowActionModel.GetAvailableActions(StudioContentLifecycleState.Approved));
        Assert.Empty(StudioWorkflowActionModel.GetAvailableActions(StudioContentLifecycleState.Published));
        Assert.Empty(StudioWorkflowActionModel.GetAvailableActions(StudioContentLifecycleState.Archived));
    }

    [Fact]
    public void Workflow_model_maps_actions_to_existing_permission_keys()
    {
        Assert.Equal(
            AdoptaPermissionKeys.AuthoringReview,
            StudioWorkflowActionModel.GetRequiredPermission(StudioWorkflowActionKind.RequestReview));
        Assert.Equal(
            AdoptaPermissionKeys.AuthoringApprove,
            StudioWorkflowActionModel.GetRequiredPermission(StudioWorkflowActionKind.Approve));
        Assert.Equal(
            AdoptaPermissionKeys.AuthoringReview,
            StudioWorkflowActionModel.GetRequiredPermission(StudioWorkflowActionKind.Reject));
    }

    [Fact]
    public void Workflow_model_validates_allowed_transition()
    {
        var content = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Draft);
        var model = StudioWorkflowActionModel.FromContent(content);

        var validation = model.Validate(StudioWorkflowActionKind.RequestReview);

        Assert.True(validation.Succeeded);
        Assert.Empty(validation.Issues);
    }

    [Fact]
    public void Workflow_model_rejects_invalid_transition_with_safe_issue()
    {
        var content = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Published);
        var model = StudioWorkflowActionModel.FromContent(content);

        var validation = model.Validate(StudioWorkflowActionKind.Approve);

        Assert.False(validation.Succeeded);
        var issue = Assert.Single(validation.Issues);
        Assert.Equal("invalid_lifecycle_transition", issue.Code);
        Assert.Equal("lifecycleState", issue.Path);
        Assert.DoesNotContain("token", issue.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", issue.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Workflow_model_updates_state_after_success()
    {
        var content = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.InReview);
        var model = StudioWorkflowActionModel.FromContent(content);

        model.MarkSucceeded(StudioWorkflowActionKind.Approve);

        Assert.Equal(StudioWorkflowActionState.Succeeded, model.State);
        Assert.Equal(StudioContentLifecycleState.Approved, model.LifecycleState);
        Assert.Equal(StudioWorkflowActionKind.Approve, model.LastAction);
        Assert.Equal("Content approved locally.", model.SafeMessage);
    }
}
