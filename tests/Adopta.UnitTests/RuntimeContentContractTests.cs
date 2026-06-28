using Adopta.Application.Runtime;

namespace Adopta.UnitTests;

public sealed class RuntimeContentContractTests
{
    [Fact]
    public void Valid_runtime_content_item_has_no_validation_issues()
    {
        var issues = RuntimeContentValidator.ValidateItem(BuildItem());

        Assert.Empty(issues);
    }

    [Fact]
    public void Invalid_runtime_content_item_returns_typed_validation_issues()
    {
        var item = BuildItem(id: "", version: "", title: "");

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Contains(issues, issue => issue.Code == "invalid_content_item" && issue.Path == "item.id");
        Assert.Contains(issues, issue => issue.Code == "invalid_content_item" && issue.Path == "item.version");
        Assert.Contains(issues, issue => issue.Code == "invalid_content_item" && issue.Path == "item.title");
    }

    [Fact]
    public void Valid_runtime_content_bundle_has_no_validation_issues()
    {
        var issues = RuntimeContentValidator.ValidateBundle(BuildBundle());

        Assert.Empty(issues);
    }

    [Fact]
    public void Invalid_runtime_content_bundle_returns_typed_validation_issues()
    {
        var bundle = BuildBundle(
            bundleId: "",
            tenantId: "",
            applicationId: "",
            environment: "prod",
            channel: "live",
            version: "");

        var issues = RuntimeContentValidator.ValidateBundle(bundle);

        Assert.Contains(issues, issue => issue.Code == "invalid_content_bundle" && issue.Path == "bundle.bundleId");
        Assert.Contains(issues, issue => issue.Code == "invalid_content_bundle" && issue.Path == "bundle.environment");
        Assert.Contains(issues, issue => issue.Code == "invalid_content_bundle" && issue.Path == "bundle.channel");
    }

    [Fact]
    public void Runtime_content_bundle_rejects_duplicate_item_ids()
    {
        var bundle = BuildBundle(items:
        [
            BuildItem(id: "item-1"),
            BuildItem(id: "item-1", type: RuntimeContentType.Callout)
        ]);

        var issues = RuntimeContentValidator.ValidateBundle(bundle);

        Assert.Contains(issues, issue => issue.Code == "duplicate_content_item_id");
    }

    [Fact]
    public void Runtime_content_item_rejects_invalid_anchor_descriptor()
    {
        var item = BuildItem(anchor: new RuntimeAnchorDescriptor("css", ".submit"));

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Contains(issues, issue => issue.Code == "invalid_anchor_descriptor");
    }

    [Fact]
    public void Runtime_content_item_validates_targeting_placeholder_shape()
    {
        var valid = BuildItem(targeting: new RuntimeTargetingPlaceholder("placeholder", ["segment-a"], ["billing"]));
        var invalid = BuildItem(targeting: new RuntimeTargetingPlaceholder("evaluate-now", [], []));

        Assert.Empty(RuntimeContentValidator.ValidateItem(valid));
        Assert.Contains(RuntimeContentValidator.ValidateItem(invalid), issue => issue.Code == "invalid_targeting_placeholder");
    }

    private static RuntimeContentBundle BuildBundle(
        string bundleId = "bundle-1",
        string tenantId = "tenant-1",
        string applicationId = "app-1",
        string environment = "production",
        string channel = "published",
        string version = "2026.06.28",
        IReadOnlyCollection<RuntimeContentItem>? items = null)
    {
        return new RuntimeContentBundle(
            bundleId,
            tenantId,
            applicationId,
            environment,
            channel,
            version,
            DateTimeOffset.UtcNow,
            items ?? [BuildItem()]);
    }

    private static RuntimeContentItem BuildItem(
        string id = "item-1",
        RuntimeContentType type = RuntimeContentType.Tooltip,
        string version = "1.0.0",
        string title = "Submit return",
        RuntimeAnchorDescriptor? anchor = null,
        RuntimeTargetingPlaceholder? targeting = null)
    {
        return new RuntimeContentItem(
            id,
            type,
            version,
            title,
            "Use this action when the return is ready.",
            anchor ?? new RuntimeAnchorDescriptor("data-adopt-id", "billing.submit"),
            targeting ?? new RuntimeTargetingPlaceholder("placeholder", [], []));
    }
}
