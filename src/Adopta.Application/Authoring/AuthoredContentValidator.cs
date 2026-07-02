using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public static class AuthoredContentValidator
{
    public static AuthoredContentValidationResult ValidateContent(AuthoredContentContract? content)
    {
        var issues = new List<AuthoredContentValidationIssue>();

        if (content is null)
        {
            issues.Add(Issue("invalid_authored_content", "content", "Authored content is invalid."));
            return AuthoredContentValidationResult.Failure(issues);
        }

        RequireGuid(content.Id, "content.id", "invalid_authored_content", issues);
        RequireGuid(content.TenantId, "content.tenantId", "invalid_authored_content", issues);
        RequireGuid(content.ApplicationId, "content.applicationId", "invalid_authored_content", issues);
        RequireContentType(content.ContentType, "content.contentType", issues);
        RequireNonBlank(content.ContentKey, "content.contentKey", "invalid_authored_content", issues);
        RequireNonBlank(content.Title, "content.title", "invalid_authored_content", issues);

        if (content.Versions is null)
        {
            issues.Add(Issue("invalid_authored_content", "content.versions", "Content versions are required."));
        }
        else
        {
            var seenVersions = new HashSet<string>(StringComparer.Ordinal);
            var index = 0;
            foreach (var version in content.Versions)
            {
                issues.AddRange(ValidateVersion(version, $"content.versions[{index}]").Issues);

                if (!string.IsNullOrWhiteSpace(version.Version) && !seenVersions.Add(version.Version.Trim()))
                {
                    issues.Add(Issue("duplicate_content_version", $"content.versions[{index}].version", "Content version is duplicated."));
                }

                index++;
            }
        }

        return issues.Count == 0
            ? AuthoredContentValidationResult.Success()
            : AuthoredContentValidationResult.Failure(issues);
    }

    public static AuthoredContentValidationResult ValidateVersion(
        AuthoredContentVersionContract? version,
        string path = "version")
    {
        var issues = new List<AuthoredContentValidationIssue>();

        if (version is null)
        {
            issues.Add(Issue("invalid_content_version", path, "Content version is invalid."));
            return AuthoredContentValidationResult.Failure(issues);
        }

        RequireGuid(version.Id, $"{path}.id", "invalid_content_version", issues);
        RequireNonBlank(version.Version, $"{path}.version", "invalid_content_version", issues);

        if (!Enum.IsDefined(version.LifecycleState))
        {
            issues.Add(Issue("invalid_lifecycle_state", $"{path}.lifecycleState", "Lifecycle state is invalid."));
        }

        if (version.CreatedAtUtc == default)
        {
            issues.Add(Issue("invalid_content_version", $"{path}.createdAtUtc", "Created timestamp is required."));
        }

        return issues.Count == 0
            ? AuthoredContentValidationResult.Success()
            : AuthoredContentValidationResult.Failure(issues);
    }

    public static AuthoredContentValidationResult ValidateTransition(
        ContentLifecycleState from,
        ContentLifecycleState to)
    {
        var issues = new List<AuthoredContentValidationIssue>();

        if (!Enum.IsDefined(from))
        {
            issues.Add(Issue("invalid_lifecycle_state", "transition.from", "Lifecycle state is invalid."));
        }

        if (!Enum.IsDefined(to))
        {
            issues.Add(Issue("invalid_lifecycle_state", "transition.to", "Lifecycle state is invalid."));
        }

        if (issues.Count == 0 && !ContentLifecycleTransition.IsAllowed(from, to))
        {
            issues.Add(Issue("invalid_lifecycle_transition", "transition", "Lifecycle transition is not allowed."));
        }

        return issues.Count == 0
            ? AuthoredContentValidationResult.Success()
            : AuthoredContentValidationResult.Failure(issues);
    }

    private static void RequireGuid(
        Guid value,
        string path,
        string code,
        ICollection<AuthoredContentValidationIssue> issues)
    {
        if (value == Guid.Empty)
        {
            issues.Add(Issue(code, path, "Value is required."));
        }
    }

    private static void RequireNonBlank(
        string? value,
        string path,
        string code,
        ICollection<AuthoredContentValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(Issue(code, path, "Value must be a non-empty string."));
        }
    }

    private static void RequireContentType(
        AuthoredContentType? contentType,
        string path,
        ICollection<AuthoredContentValidationIssue> issues)
    {
        if (contentType is null || !Enum.IsDefined(contentType.Value))
        {
            issues.Add(Issue("invalid_content_type", path, "Content type is invalid."));
        }
    }

    private static AuthoredContentValidationIssue Issue(
        string code,
        string path,
        string message)
    {
        return new AuthoredContentValidationIssue(code, path, message);
    }
}
