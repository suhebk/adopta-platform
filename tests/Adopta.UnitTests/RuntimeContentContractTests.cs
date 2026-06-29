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

    [Fact]
    public void Runtime_content_item_accepts_valid_checklist_contract()
    {
        var item = BuildItem(
            type: RuntimeContentType.Checklist,
            checklist: new RuntimeChecklistContent(
            [
                new RuntimeChecklistStep(
                    "step-1",
                    "Confirm details",
                    "Complete this step when ready.",
                    new RuntimeAnchorDescriptor("data-adopt-id", "billing.confirm"),
                    BuildExperience()),
                new RuntimeChecklistStep("step-2", "Submit return")
            ]));

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Empty(issues);
    }

    [Fact]
    public void Runtime_content_item_rejects_invalid_checklist_contract()
    {
        var item = BuildItem(
            type: RuntimeContentType.Checklist,
            checklist: new RuntimeChecklistContent(
            [
                new RuntimeChecklistStep("step-1", ""),
                new RuntimeChecklistStep("step-1", "Duplicate", Anchor: new RuntimeAnchorDescriptor("css", ".submit"))
            ]));

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Contains(issues, issue => issue.Code == "invalid_checklist_content");
        Assert.Contains(issues, issue => issue.Code == "duplicate_checklist_step_id");
        Assert.Contains(issues, issue => issue.Code == "invalid_anchor_descriptor");
    }

    [Fact]
    public void Runtime_content_item_accepts_valid_walkthrough_contract()
    {
        var item = BuildItem(
            type: RuntimeContentType.Walkthrough,
            walkthrough: new RuntimeWalkthroughContent(
            [
                new RuntimeWalkthroughStep("intro", "Start here", Experience: BuildExperience()),
                new RuntimeWalkthroughStep(
                    "submit",
                    "Submit return",
                    "Use this step when ready.",
                    new RuntimeAnchorDescriptor("data-adopt-id", "billing.submit"))
            ]));

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Empty(issues);
    }

    [Fact]
    public void Runtime_content_item_rejects_invalid_walkthrough_contract()
    {
        var item = BuildItem(
            type: RuntimeContentType.Walkthrough,
            walkthrough: new RuntimeWalkthroughContent(
            [
                new RuntimeWalkthroughStep("", ""),
                new RuntimeWalkthroughStep("intro", "Intro"),
                new RuntimeWalkthroughStep("intro", "Duplicate intro")
            ]));

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Contains(issues, issue => issue.Code == "invalid_walkthrough_content");
        Assert.Contains(issues, issue => issue.Code == "duplicate_walkthrough_step_id");
    }

    [Fact]
    public void Runtime_content_item_validates_experience_metadata_values()
    {
        var item = BuildItem(experience: new RuntimeExperienceMetadata(
            new RuntimeRendererPlacement("coordinate-100", ["bottom"]),
            ["dismiss-button", "dismiss-button"],
            new RuntimeRendererTheme("raw-css-red", "comfortable", "standard")));

        var issues = RuntimeContentValidator.ValidateItem(item);

        Assert.Contains(issues, issue => issue.Code == "invalid_renderer_placement");
        Assert.Contains(issues, issue => issue.Code == "invalid_dismiss_behavior");
        Assert.Contains(issues, issue => issue.Code == "invalid_renderer_theme");
    }

    [Fact]
    public void Runtime_content_validation_messages_do_not_echo_sensitive_markers()
    {
        var marker = string.Concat("Bear", "er");
        var sensitiveValue = $"{marker} runtime-marker";
        var item = BuildItem(experience: new RuntimeExperienceMetadata(
            Theme: new RuntimeRendererTheme(sensitiveValue)));

        var issues = RuntimeContentValidator.ValidateItem(item);
        var messages = string.Join(' ', issues.Select(issue => issue.Message));

        Assert.NotEmpty(issues);
        Assert.DoesNotContain(marker, messages, StringComparison.Ordinal);
        Assert.DoesNotContain("runtime-marker", messages, StringComparison.Ordinal);
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
        RuntimeTargetingPlaceholder? targeting = null,
        RuntimeExperienceMetadata? experience = null,
        RuntimeChecklistContent? checklist = null,
        RuntimeWalkthroughContent? walkthrough = null)
    {
        return new RuntimeContentItem(
            id,
            type,
            version,
            title,
            "Use this action when the return is ready.",
            anchor ?? new RuntimeAnchorDescriptor("data-adopt-id", "billing.submit"),
            targeting ?? new RuntimeTargetingPlaceholder("placeholder", [], []),
            experience,
            checklist,
            walkthrough);
    }

    private static RuntimeExperienceMetadata BuildExperience()
    {
        return new RuntimeExperienceMetadata(
            new RuntimeRendererPlacement("right", ["bottom", "inline"]),
            ["dismiss-button", "escape-key"],
            new RuntimeRendererTheme("info", "comfortable", "standard"));
    }
}
