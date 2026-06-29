using Adopta.Application.Authoring;
using Adopta.Application.Runtime;
using Adopta.Domain.Authoring;

namespace Adopta.UnitTests;

public sealed class AuthoringRuntimeBundleMapperTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Approved_authored_content_maps_to_runtime_bundle_contract()
    {
        var content = BuildContent(ContentLifecycleState.Approved);
        var version = Assert.Single(content.Versions);
        var command = BuildCommand(content, version);

        var bundle = AuthoredContentRuntimeBundleMapper.Map(content, version, command);

        Assert.Equal(content.TenantId, bundle.TenantId);
        Assert.Equal(content.ApplicationId, bundle.ApplicationId);
        Assert.Equal("production", bundle.Environment);
        Assert.Equal(DeliveryChannel.Published, bundle.Channel);
        Assert.Equal(content.TenantId.ToString(), bundle.Content.TenantId);
        Assert.Equal(content.ApplicationId.ToString(), bundle.Content.ApplicationId);
        Assert.Equal("production", bundle.Content.Environment);
        Assert.Equal("published", bundle.Content.Channel);
        Assert.Equal(version.Version, bundle.Content.Version);
        Assert.Equal(Now, bundle.Content.GeneratedAtUtc);
        Assert.Single(bundle.Content.Items);
    }

    [Fact]
    public void Mapped_bundle_content_is_minimal_and_safe()
    {
        var content = BuildContent(ContentLifecycleState.Approved);
        var version = Assert.Single(content.Versions);
        var command = BuildCommand(content, version);

        var bundle = AuthoredContentRuntimeBundleMapper.Map(content, version, command);
        var item = Assert.Single(bundle.Content.Items);

        Assert.Equal(content.ContentKey, item.Id);
        Assert.Equal(content.Title, item.Title);
        Assert.Equal(version.Version, item.Version);
        Assert.Null(item.Body);
        Assert.NotNull(item.Anchor);
        Assert.Equal("data-adopt-id", item.Anchor.Strategy);
        Assert.Equal(content.ContentKey, item.Anchor.Value);
        Assert.NotNull(item.Targeting);
        Assert.Equal("placeholder", item.Targeting.Mode);
        Assert.Empty(item.Targeting.Segments);
        Assert.Empty(item.Targeting.PageKeys);
    }

    [Theory]
    [InlineData("billing submit")]
    [InlineData("billing/submit")]
    [InlineData("<billing.submit>")]
    public void Unsafe_content_key_anchor_metadata_fails_safely(string contentKey)
    {
        var content = BuildContent(ContentLifecycleState.Approved, contentKey);
        var version = Assert.Single(content.Versions);
        var command = BuildCommand(content, version);

        var issues = AuthoredContentRuntimeBundleMapper.ValidateMappingInputs(content, version, command);

        Assert.Contains(issues, issue => issue.Code == "unsafe_content_key");
        Assert.DoesNotContain(issues, issue => issue.Message.Contains(contentKey, StringComparison.Ordinal));
    }

    [Fact]
    public void Mapping_validation_rejects_invalid_environment_and_channel()
    {
        var content = BuildContent(ContentLifecycleState.Approved);
        var version = Assert.Single(content.Versions);
        var command = BuildCommand(content, version) with
        {
            Environment = "live",
            Channel = (DeliveryChannel)99
        };

        var issues = AuthoredContentRuntimeBundleMapper.ValidateMappingInputs(content, version, command);

        Assert.Contains(issues, issue => issue.Code == "invalid_publish_environment");
        Assert.Contains(issues, issue => issue.Code == "invalid_publish_channel");
    }

    private static AuthoredContentItem BuildContent(
        ContentLifecycleState state,
        string contentKey = "billing.submit")
    {
        return new AuthoredContentItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            contentKey,
            "Submit return",
            [
                new AuthoredContentVersion(
                    Guid.NewGuid(),
                    "1.0.0",
                    state,
                    Now)
            ]);
    }

    private static AuthoredContentPublishCommand BuildCommand(
        AuthoredContentItem content,
        AuthoredContentVersion version)
    {
        return new AuthoredContentPublishCommand(
            content.TenantId,
            content.Id,
            version.Id,
            Guid.NewGuid(),
            "production",
            DeliveryChannel.Published,
            Now);
    }
}
