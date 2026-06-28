namespace Adopta.Application.Runtime;

public static class RuntimeContentValidator
{
    private static readonly string[] AllowedEnvironments = ["development", "test", "production"];
    private static readonly string[] AllowedChannels = ["preview", "published"];

    public static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateItem(RuntimeContentItem? item, string path = "item")
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (item is null)
        {
            issues.Add(Issue("invalid_content_item", path, "Content item is invalid."));
            return issues;
        }

        RequireNonBlank(item.Id, $"{path}.id", "invalid_content_item", issues);
        RequireNonBlank(item.Version, $"{path}.version", "invalid_content_item", issues);
        RequireNonBlank(item.Title, $"{path}.title", "invalid_content_item", issues);

        if (item.Anchor is not null && !IsValidAnchor(item.Anchor))
        {
            issues.Add(Issue("invalid_anchor_descriptor", $"{path}.anchor", "Anchor descriptor is invalid."));
        }

        if (item.Targeting is not null && !IsValidTargeting(item.Targeting))
        {
            issues.Add(Issue("invalid_targeting_placeholder", $"{path}.targeting", "Targeting placeholder is invalid."));
        }

        return issues;
    }

    public static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateBundle(RuntimeContentBundle? bundle)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (bundle is null)
        {
            issues.Add(Issue("invalid_content_bundle", "bundle", "Content bundle is invalid."));
            return issues;
        }

        RequireNonBlank(bundle.BundleId, "bundle.bundleId", "invalid_content_bundle", issues);
        RequireNonBlank(bundle.TenantId, "bundle.tenantId", "invalid_content_bundle", issues);
        RequireNonBlank(bundle.ApplicationId, "bundle.applicationId", "invalid_content_bundle", issues);
        RequireAllowed(bundle.Environment, AllowedEnvironments, "bundle.environment", issues);
        RequireAllowed(bundle.Channel, AllowedChannels, "bundle.channel", issues);
        RequireNonBlank(bundle.Version, "bundle.version", "invalid_content_bundle", issues);

        if (bundle.Items is null)
        {
            issues.Add(Issue("invalid_content_bundle", "bundle.items", "Content bundle items are required."));
            return issues;
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;
        foreach (var item in bundle.Items)
        {
            issues.AddRange(ValidateItem(item, $"bundle.items[{index}]"));

            if (!string.IsNullOrWhiteSpace(item.Id) && !seenIds.Add(item.Id.Trim()))
            {
                issues.Add(Issue("duplicate_content_item_id", $"bundle.items[{index}].id", "Content item id is duplicated."));
            }

            index++;
        }

        return issues;
    }

    private static bool IsValidAnchor(RuntimeAnchorDescriptor anchor)
    {
        return anchor.Strategy == "data-adopt-id" && !string.IsNullOrWhiteSpace(anchor.Value);
    }

    private static bool IsValidTargeting(RuntimeTargetingPlaceholder targeting)
    {
        return targeting.Mode == "placeholder"
            && targeting.Segments.All(value => !string.IsNullOrWhiteSpace(value))
            && targeting.PageKeys.All(value => !string.IsNullOrWhiteSpace(value));
    }

    private static void RequireNonBlank(
        string? value,
        string path,
        string code,
        ICollection<RuntimeContentValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(Issue(code, path, "Value must be a non-empty string."));
        }
    }

    private static void RequireAllowed(
        string value,
        IReadOnlyCollection<string> allowedValues,
        string path,
        ICollection<RuntimeContentValidationIssue> issues)
    {
        if (!allowedValues.Contains(value, StringComparer.Ordinal))
        {
            issues.Add(Issue("invalid_content_bundle", path, "Value is outside the allowed contract set."));
        }
    }

    private static RuntimeContentValidationIssue Issue(string code, string path, string message)
    {
        return new RuntimeContentValidationIssue(code, path, message);
    }
}

