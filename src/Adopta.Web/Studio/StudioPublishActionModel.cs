using Adopta.Application.Identity;
using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public enum StudioPublishActionState
{
    Loading = 0,
    Ready = 1,
    Publishing = 2,
    Published = 3,
    ValidationError = 4,
    NotAuthorized = 5,
    Error = 6
}

public sealed record StudioPublishActionIssue(
    string Code,
    string Path,
    string Message);

public sealed record StudioPublishActionValidationResult(
    bool Succeeded,
    IReadOnlyCollection<StudioPublishActionIssue> Issues)
{
    public static StudioPublishActionValidationResult Success() => new(true, []);

    public static StudioPublishActionValidationResult Failure(
        IReadOnlyCollection<StudioPublishActionIssue> issues) =>
        new(false, issues);
}

public sealed class StudioPublishActionModel
{
    private static readonly string[] AllowedEnvironments = ["development", "test", "production"];

    public Guid ContentId { get; set; }

    public Guid VersionId { get; set; }

    public StudioContentLifecycleState LifecycleState { get; set; }

    public string Environment { get; set; } = "production";

    public DeliveryChannel Channel { get; set; } = DeliveryChannel.Published;

    public StudioPublishActionState State { get; set; } = StudioPublishActionState.Ready;

    public string SafeMessage { get; set; } = "Publish readiness is ready.";

    public IReadOnlyCollection<StudioPublishActionIssue> Issues { get; private set; } = [];

    public bool HasValidationIssues => Issues.Count > 0;

    public static IReadOnlyCollection<string> GetAllowedEnvironments() => AllowedEnvironments;

    public static IReadOnlyCollection<DeliveryChannel> GetAllowedChannels() =>
        Enum.GetValues<DeliveryChannel>();

    public static StudioPublishActionModel Loading() =>
        new()
        {
            State = StudioPublishActionState.Loading,
            SafeMessage = "Loading publish readiness."
        };

    public static StudioPublishActionModel FromContent(StudioContentListItem? content)
    {
        if (content?.CurrentVersion is null)
        {
            return new StudioPublishActionModel
            {
                State = StudioPublishActionState.Error,
                SafeMessage = "Publish readiness could not be loaded."
            };
        }

        return new StudioPublishActionModel
        {
            ContentId = content.Id,
            VersionId = content.CurrentVersion.Id,
            LifecycleState = content.LifecycleState,
            State = StudioPublishActionState.Ready,
            SafeMessage = GetReadinessMessage(content.LifecycleState)
        };
    }

    public static bool IsPublishAvailable(StudioContentLifecycleState lifecycleState) =>
        lifecycleState == StudioContentLifecycleState.Approved;

    public static string GetRequiredPermission() =>
        AdoptaPermissionKeys.AuthoringPublish;

    public static string GetReadinessMessage(StudioContentLifecycleState lifecycleState) =>
        lifecycleState switch
        {
            StudioContentLifecycleState.Approved => "Approved content is ready for local publish validation.",
            StudioContentLifecycleState.Draft => "Draft content must be reviewed and approved before publishing.",
            StudioContentLifecycleState.InReview => "Content in review must be approved before publishing.",
            StudioContentLifecycleState.Published => "Published content has no publish action in this view.",
            StudioContentLifecycleState.Archived => "Archived content has no publish action in this view.",
            _ => "Publish readiness is ready."
        };

    public StudioPublishActionValidationResult Validate()
    {
        var issues = new List<StudioPublishActionIssue>();

        if (ContentId == Guid.Empty)
        {
            issues.Add(Issue("required", "contentId", "Content selection is required."));
        }

        if (VersionId == Guid.Empty)
        {
            issues.Add(Issue("required", "versionId", "Version selection is required."));
        }

        if (!IsPublishAvailable(LifecycleState))
        {
            issues.Add(Issue(
                "invalid_lifecycle_transition",
                "lifecycleState",
                "Publish is not available for the selected lifecycle state."));
        }

        if (!AllowedEnvironments.Contains(Environment, StringComparer.Ordinal))
        {
            issues.Add(Issue(
                "invalid_publish_environment",
                "environment",
                "Publish environment is invalid."));
        }

        if (!Enum.IsDefined(Channel))
        {
            issues.Add(Issue(
                "invalid_publish_channel",
                "channel",
                "Publish channel is invalid."));
        }

        return issues.Count == 0
            ? StudioPublishActionValidationResult.Success()
            : StudioPublishActionValidationResult.Failure(issues);
    }

    public void ApplyValidationResult(StudioPublishActionValidationResult validation)
    {
        Issues = validation.Issues;
        State = validation.Succeeded
            ? StudioPublishActionState.Ready
            : StudioPublishActionState.ValidationError;
        SafeMessage = validation.Succeeded
            ? GetReadinessMessage(LifecycleState)
            : "Publish readiness has validation issues.";
    }

    public void MarkPublishing()
    {
        Issues = [];
        State = StudioPublishActionState.Publishing;
        SafeMessage = "Publish validation is processing.";
    }

    public void MarkPublished()
    {
        Issues = [];
        LifecycleState = StudioContentLifecycleState.Published;
        State = StudioPublishActionState.Published;
        SafeMessage = "Content published locally.";
    }

    public void MarkNotAuthorized(string safeMessage)
    {
        Issues = [];
        State = StudioPublishActionState.NotAuthorized;
        SafeMessage = safeMessage;
    }

    public void MarkError(string safeMessage)
    {
        Issues = [];
        State = StudioPublishActionState.Error;
        SafeMessage = safeMessage;
    }

    private static StudioPublishActionIssue Issue(
        string code,
        string path,
        string message) =>
        new(code, path, message);
}
