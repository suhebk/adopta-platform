using Adopta.Application.Runtime;
using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public static class AuthoredContentRuntimeBundleMapper
{
    public static IReadOnlyCollection<AuthoredContentValidationIssue> ValidateMappingInputs(
        AuthoredContentItem? content,
        AuthoredContentVersion? version,
        AuthoredContentPublishCommand command)
    {
        var issues = new List<AuthoredContentValidationIssue>();

        if (content is null)
        {
            issues.Add(Issue("authored_content_not_found", "content", "Authored content was not found."));
            return issues;
        }

        if (version is null)
        {
            issues.Add(Issue("authored_content_version_not_found", "version", "Authored content version was not found."));
            return issues;
        }

        if (content.TenantId != command.TenantId)
        {
            issues.Add(Issue("invalid_publish_scope", "command.tenantId", "Publish scope is invalid."));
        }

        if (content.ApplicationId == Guid.Empty)
        {
            issues.Add(Issue("invalid_publish_scope", "content.applicationId", "Application scope is invalid."));
        }

        if (!Enum.IsDefined(content.ContentType))
        {
            issues.Add(Issue("invalid_content_type", "content.contentType", "Content type is invalid."));
        }

        if (!IsSafeStructuralKey(content.ContentKey))
        {
            issues.Add(Issue("unsafe_content_key", "content.contentKey", "Content key is invalid."));
        }

        if (string.IsNullOrWhiteSpace(content.Title))
        {
            issues.Add(Issue("invalid_authored_content", "content.title", "Title is required."));
        }

        if (string.IsNullOrWhiteSpace(version.Version))
        {
            issues.Add(Issue("invalid_content_version", "version.version", "Version is required."));
        }

        if (!IsAllowedEnvironment(command.Environment))
        {
            issues.Add(Issue("invalid_publish_environment", "command.environment", "Publish environment is invalid."));
        }

        if (!Enum.IsDefined(command.Channel))
        {
            issues.Add(Issue("invalid_publish_channel", "command.channel", "Publish channel is invalid."));
        }

        return issues;
    }

    public static DeliveryBundle Map(
        AuthoredContentItem content,
        AuthoredContentVersion version,
        AuthoredContentPublishCommand command)
    {
        var channel = ToContractChannel(command.Channel);
        var contentItem = new RuntimeContentItem(
            content.ContentKey.Trim(),
            ToRuntimeContentType(content.ContentType),
            version.Version.Trim(),
            content.Title.Trim(),
            null,
            new RuntimeAnchorDescriptor("data-adopt-id", content.ContentKey.Trim()),
            new RuntimeTargetingPlaceholder("placeholder", [], []));

        var contentBundle = new RuntimeContentBundle(
            $"{content.Id:N}-{version.Id:N}",
            content.TenantId.ToString(),
            content.ApplicationId.ToString(),
            command.Environment.Trim(),
            channel,
            version.Version.Trim(),
            command.RequestedAtUtc,
            [contentItem]);

        return new DeliveryBundle(
            content.TenantId,
            content.ApplicationId,
            command.Environment.Trim(),
            command.Channel,
            contentBundle);
    }

    private static bool IsAllowedEnvironment(string value)
    {
        return value is "development" or "test" or "production";
    }

    private static bool IsSafeStructuralKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 128
            && trimmed.All(character =>
                char.IsAsciiLetterOrDigit(character)
                || character == '.'
                || character == '-'
                || character == '_');
    }

    private static string ToContractChannel(DeliveryChannel channel)
    {
        return channel switch
        {
            DeliveryChannel.Preview => "preview",
            DeliveryChannel.Published => "published",
            _ => string.Empty
        };
    }

    private static RuntimeContentType ToRuntimeContentType(AuthoredContentType contentType)
    {
        return contentType switch
        {
            AuthoredContentType.Tooltip => RuntimeContentType.Tooltip,
            AuthoredContentType.Callout => RuntimeContentType.Callout,
            AuthoredContentType.Checklist => RuntimeContentType.Checklist,
            AuthoredContentType.Walkthrough => RuntimeContentType.Walkthrough,
            _ => throw new InvalidOperationException("Content type is invalid.")
        };
    }

    private static AuthoredContentValidationIssue Issue(
        string code,
        string path,
        string message)
    {
        return new AuthoredContentValidationIssue(code, path, message);
    }
}
