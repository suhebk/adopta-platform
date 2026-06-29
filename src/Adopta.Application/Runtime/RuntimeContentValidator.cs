namespace Adopta.Application.Runtime;

public static class RuntimeContentValidator
{
    private static readonly string[] AllowedEnvironments = ["development", "test", "production"];
    private static readonly string[] AllowedChannels = ["preview", "published"];
    private static readonly string[] AllowedPlacements = ["auto", "top", "right", "bottom", "left", "center", "inline", "banner"];
    private static readonly string[] AllowedDismissBehaviors = ["dismiss-button", "escape-key", "outside-click", "auto-timeout"];
    private static readonly string[] AllowedThemeTones = ["neutral", "info", "success", "warning", "critical"];
    private static readonly string[] AllowedThemeDensities = ["comfortable", "compact"];
    private static readonly string[] AllowedThemeEmphasis = ["subtle", "standard", "strong"];

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

        if (item.Experience is not null)
        {
            issues.AddRange(ValidateExperience(item.Experience, $"{path}.experience"));
        }

        if (item.Type == RuntimeContentType.Checklist && item.Checklist is not null)
        {
            issues.AddRange(ValidateChecklist(item.Checklist, $"{path}.checklist"));
        }

        if (item.Type == RuntimeContentType.Walkthrough && item.Walkthrough is not null)
        {
            issues.AddRange(ValidateWalkthrough(item.Walkthrough, $"{path}.walkthrough"));
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

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateChecklist(
        RuntimeChecklistContent checklist,
        string path)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (checklist.Steps is null || checklist.Steps.Count == 0)
        {
            issues.Add(Issue("invalid_checklist_content", $"{path}.steps", "Content steps are required."));
            return issues;
        }

        ValidateSteps(
            checklist.Steps,
            path,
            "invalid_checklist_content",
            "duplicate_checklist_step_id",
            step => step.Id,
            step => step.Title,
            step => step.Anchor,
            step => step.Experience,
            issues);

        return issues;
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateWalkthrough(
        RuntimeWalkthroughContent walkthrough,
        string path)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (walkthrough.Steps is null || walkthrough.Steps.Count == 0)
        {
            issues.Add(Issue("invalid_walkthrough_content", $"{path}.steps", "Content steps are required."));
            return issues;
        }

        ValidateSteps(
            walkthrough.Steps,
            path,
            "invalid_walkthrough_content",
            "duplicate_walkthrough_step_id",
            step => step.Id,
            step => step.Title,
            step => step.Anchor,
            step => step.Experience,
            issues);

        return issues;
    }

    private static void ValidateSteps<TStep>(
        IReadOnlyCollection<TStep> steps,
        string path,
        string invalidCode,
        string duplicateCode,
        Func<TStep, string> id,
        Func<TStep, string> title,
        Func<TStep, RuntimeAnchorDescriptor?> anchor,
        Func<TStep, RuntimeExperienceMetadata?> experience,
        ICollection<RuntimeContentValidationIssue> issues)
    {
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;
        foreach (var step in steps)
        {
            var stepPath = $"{path}.steps[{index}]";
            RequireNonBlank(id(step), $"{stepPath}.id", invalidCode, issues);
            RequireNonBlank(title(step), $"{stepPath}.title", invalidCode, issues);

            var stepAnchor = anchor(step);
            if (stepAnchor is not null && !IsValidAnchor(stepAnchor))
            {
                issues.Add(Issue("invalid_anchor_descriptor", $"{stepPath}.anchor", "Anchor descriptor is invalid."));
            }

            var stepExperience = experience(step);
            if (stepExperience is not null)
            {
                foreach (var issue in ValidateExperience(stepExperience, $"{stepPath}.experience"))
                {
                    issues.Add(issue);
                }
            }

            var normalizedId = id(step).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedId) && !seenIds.Add(normalizedId))
            {
                issues.Add(Issue(duplicateCode, $"{stepPath}.id", "Content step id is duplicated."));
            }

            index++;
        }
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateExperience(
        RuntimeExperienceMetadata experience,
        string path)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (experience.Placement is not null)
        {
            issues.AddRange(ValidatePlacement(experience.Placement, $"{path}.placement"));
        }

        if (experience.DismissBehavior is not null)
        {
            issues.AddRange(ValidateDismissBehavior(experience.DismissBehavior, $"{path}.dismissBehavior"));
        }

        if (experience.Theme is not null)
        {
            issues.AddRange(ValidateTheme(experience.Theme, $"{path}.theme"));
        }

        return issues;
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidatePlacement(
        RuntimeRendererPlacement placement,
        string path)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (!AllowedPlacements.Contains(placement.Preferred, StringComparer.Ordinal))
        {
            issues.Add(Issue("invalid_renderer_placement", path, "Renderer placement is invalid."));
        }

        if (placement.Fallback is not null &&
            placement.Fallback.Any(value => !AllowedPlacements.Contains(value, StringComparer.Ordinal)))
        {
            issues.Add(Issue("invalid_renderer_placement", $"{path}.fallback", "Renderer placement fallback is invalid."));
        }

        return issues;
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateDismissBehavior(
        IReadOnlyCollection<string> dismissBehavior,
        string path)
    {
        if (dismissBehavior.Count == 0 ||
            dismissBehavior.Any(value => !AllowedDismissBehaviors.Contains(value, StringComparer.Ordinal)) ||
            dismissBehavior.Distinct(StringComparer.Ordinal).Count() != dismissBehavior.Count)
        {
            return [Issue("invalid_dismiss_behavior", path, "Dismiss behavior is invalid.")];
        }

        return [];
    }

    private static IReadOnlyCollection<RuntimeContentValidationIssue> ValidateTheme(
        RuntimeRendererTheme theme,
        string path)
    {
        var issues = new List<RuntimeContentValidationIssue>();

        if (theme.Tone is not null && !AllowedThemeTones.Contains(theme.Tone, StringComparer.Ordinal))
        {
            issues.Add(Issue("invalid_renderer_theme", $"{path}.tone", "Renderer theme is invalid."));
        }

        if (theme.Density is not null && !AllowedThemeDensities.Contains(theme.Density, StringComparer.Ordinal))
        {
            issues.Add(Issue("invalid_renderer_theme", $"{path}.density", "Renderer theme is invalid."));
        }

        if (theme.Emphasis is not null && !AllowedThemeEmphasis.Contains(theme.Emphasis, StringComparer.Ordinal))
        {
            issues.Add(Issue("invalid_renderer_theme", $"{path}.emphasis", "Renderer theme is invalid."));
        }

        return issues;
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

