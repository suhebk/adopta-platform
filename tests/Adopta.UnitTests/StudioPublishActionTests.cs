using Adopta.Application.Identity;
using Adopta.Application.Runtime;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioPublishActionTests
{
    [Fact]
    public void Publish_model_exposes_publish_action_only_for_approved_content()
    {
        Assert.False(StudioPublishActionModel.IsPublishAvailable(StudioContentLifecycleState.Draft));
        Assert.False(StudioPublishActionModel.IsPublishAvailable(StudioContentLifecycleState.InReview));
        Assert.True(StudioPublishActionModel.IsPublishAvailable(StudioContentLifecycleState.Approved));
        Assert.False(StudioPublishActionModel.IsPublishAvailable(StudioContentLifecycleState.Published));
        Assert.False(StudioPublishActionModel.IsPublishAvailable(StudioContentLifecycleState.Archived));
    }

    [Fact]
    public void Publish_model_uses_existing_publish_permission_key()
    {
        Assert.Equal(
            AdoptaPermissionKeys.AuthoringPublish,
            StudioPublishActionModel.GetRequiredPermission());
    }

    [Fact]
    public void Publish_model_validates_approved_content_with_existing_environment_and_channel_contracts()
    {
        var content = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Approved);
        var model = StudioPublishActionModel.FromContent(content);

        var validation = model.Validate();

        Assert.True(validation.Succeeded);
        Assert.Empty(validation.Issues);
        Assert.Equal(["development", "test", "production"], StudioPublishActionModel.GetAllowedEnvironments());
        Assert.Equal([DeliveryChannel.Preview, DeliveryChannel.Published], StudioPublishActionModel.GetAllowedChannels());
    }

    [Theory]
    [InlineData(StudioContentLifecycleState.Draft)]
    [InlineData(StudioContentLifecycleState.InReview)]
    [InlineData(StudioContentLifecycleState.Published)]
    [InlineData(StudioContentLifecycleState.Archived)]
    public void Publish_model_rejects_invalid_lifecycle_states_safely(
        StudioContentLifecycleState lifecycleState)
    {
        var model = new StudioPublishActionModel
        {
            ContentId = Guid.NewGuid(),
            VersionId = Guid.NewGuid(),
            LifecycleState = lifecycleState
        };

        var validation = model.Validate();

        Assert.False(validation.Succeeded);
        Assert.Contains(validation.Issues, issue => issue.Code == "invalid_lifecycle_transition");
        Assert.All(validation.Issues, issue => AssertSafeMessage(issue.Message));
    }

    [Fact]
    public void Publish_model_rejects_invalid_environment_and_channel_without_echoing_values()
    {
        var model = new StudioPublishActionModel
        {
            ContentId = Guid.NewGuid(),
            VersionId = Guid.NewGuid(),
            LifecycleState = StudioContentLifecycleState.Approved,
            Environment = "Bearer token value",
            Channel = (DeliveryChannel)999
        };

        var validation = model.Validate();

        Assert.False(validation.Succeeded);
        Assert.Contains(validation.Issues, issue => issue.Code == "invalid_publish_environment");
        Assert.Contains(validation.Issues, issue => issue.Code == "invalid_publish_channel");
        Assert.All(validation.Issues, issue => AssertSafeMessage(issue.Message));
    }

    [Fact]
    public void Publish_model_marks_published_with_safe_status()
    {
        var content = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Approved);
        var model = StudioPublishActionModel.FromContent(content);

        model.MarkPublished();

        Assert.Equal(StudioPublishActionState.Published, model.State);
        Assert.Equal(StudioContentLifecycleState.Published, model.LifecycleState);
        Assert.Equal("Content published locally.", model.SafeMessage);
        Assert.Empty(model.Issues);
    }

    private static void AssertSafeMessage(string message)
    {
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.DoesNotContain("token", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer", message, StringComparison.OrdinalIgnoreCase);
    }
}
