using Adopta.Application.Abstractions.Authoring;
using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public sealed class AuthoredContentPublishingWorkflow
{
    public async Task<AuthoredContentPublishResult> PublishAsync(
        IAuthoredContentRepository repository,
        AuthoredContentPublishCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var commandIssues = ValidateCommand(command);
        if (commandIssues.Count > 0)
        {
            return AuthoredContentPublishResult.Failed(
                AuthoredContentPublishStatus.InvalidRequest,
                commandIssues);
        }

        var content = await repository.GetByIdAsync(
            command.TenantId,
            command.ContentId,
            cancellationToken);

        if (content is null)
        {
            return AuthoredContentPublishResult.Failed(
                AuthoredContentPublishStatus.NotFound,
                [Issue("authored_content_not_found", "content", "Authored content was not found.")]);
        }

        var version = content.Versions.SingleOrDefault(candidate => candidate.Id == command.VersionId);
        if (version is null)
        {
            return AuthoredContentPublishResult.Failed(
                AuthoredContentPublishStatus.NotFound,
                [Issue("authored_content_version_not_found", "version", "Authored content version was not found.")]);
        }

        var eligibilityIssues = ValidateEligibility(version);
        if (eligibilityIssues.Count > 0)
        {
            return AuthoredContentPublishResult.Failed(
                AuthoredContentPublishStatus.ValidationFailed,
                eligibilityIssues);
        }

        var mappingIssues = AuthoredContentRuntimeBundleMapper.ValidateMappingInputs(
            content,
            version,
            command);
        if (mappingIssues.Count > 0)
        {
            return AuthoredContentPublishResult.Failed(
                AuthoredContentPublishStatus.ValidationFailed,
                mappingIssues);
        }

        var bundle = AuthoredContentRuntimeBundleMapper.Map(content, version, command);
        var auditRecord = new AuthoredContentPublishingAuditRecord(
            command.TenantId,
            command.ContentId,
            command.VersionId,
            command.ActorUserId,
            command.Environment.Trim(),
            command.Channel,
            "Succeeded",
            command.RequestedAtUtc);

        return AuthoredContentPublishResult.Succeeded(bundle, auditRecord);
    }

    private static IReadOnlyCollection<AuthoredContentValidationIssue> ValidateCommand(
        AuthoredContentPublishCommand command)
    {
        var issues = new List<AuthoredContentValidationIssue>();

        RequireGuid(command.TenantId, "command.tenantId", issues);
        RequireGuid(command.ContentId, "command.contentId", issues);
        RequireGuid(command.VersionId, "command.versionId", issues);
        RequireGuid(command.ActorUserId, "command.actorUserId", issues);

        if (string.IsNullOrWhiteSpace(command.Environment))
        {
            issues.Add(Issue("invalid_publish_environment", "command.environment", "Publish environment is required."));
        }

        if (!Enum.IsDefined(command.Channel))
        {
            issues.Add(Issue("invalid_publish_channel", "command.channel", "Publish channel is invalid."));
        }

        if (command.RequestedAtUtc == default)
        {
            issues.Add(Issue("invalid_publish_timestamp", "command.requestedAtUtc", "Publish timestamp is required."));
        }

        return issues;
    }

    private static IReadOnlyCollection<AuthoredContentValidationIssue> ValidateEligibility(
        AuthoredContentVersion version)
    {
        if (version.LifecycleState == ContentLifecycleState.Approved)
        {
            return [];
        }

        var code = version.LifecycleState switch
        {
            ContentLifecycleState.Draft => "publish_denied_draft",
            ContentLifecycleState.InReview => "publish_denied_in_review",
            ContentLifecycleState.Published => "publish_denied_already_published",
            ContentLifecycleState.Archived => "publish_denied_archived",
            _ => "publish_denied_invalid_state"
        };

        return [Issue(code, "version.lifecycleState", "Authored content version is not eligible for publishing.")];
    }

    private static void RequireGuid(
        Guid value,
        string path,
        ICollection<AuthoredContentValidationIssue> issues)
    {
        if (value == Guid.Empty)
        {
            issues.Add(Issue("invalid_publish_command", path, "Value is required."));
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
