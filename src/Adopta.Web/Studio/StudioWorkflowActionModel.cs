using Adopta.Application.Identity;

namespace Adopta.Web.Studio;

public enum StudioWorkflowActionKind
{
    RequestReview = 0,
    Approve = 1,
    Reject = 2
}

public enum StudioWorkflowActionState
{
    Loading = 0,
    Ready = 1,
    Processing = 2,
    Succeeded = 3,
    ValidationError = 4,
    NotAuthorized = 5,
    Error = 6
}

public sealed record StudioWorkflowActionIssue(
    string Code,
    string Path,
    string Message);

public sealed record StudioWorkflowActionValidationResult(
    bool Succeeded,
    IReadOnlyCollection<StudioWorkflowActionIssue> Issues)
{
    public static StudioWorkflowActionValidationResult Success() => new(true, []);

    public static StudioWorkflowActionValidationResult Failure(
        IReadOnlyCollection<StudioWorkflowActionIssue> issues) =>
        new(false, issues);
}

public sealed class StudioWorkflowActionModel
{
    public Guid ContentId { get; set; }

    public Guid VersionId { get; set; }

    public StudioContentLifecycleState LifecycleState { get; set; }

    public StudioWorkflowActionKind? LastAction { get; set; }

    public StudioWorkflowActionState State { get; set; } = StudioWorkflowActionState.Ready;

    public string SafeMessage { get; set; } = "Review workflow is ready.";

    public IReadOnlyCollection<StudioWorkflowActionIssue> Issues { get; private set; } = [];

    public bool HasValidationIssues => Issues.Count > 0;

    public static StudioWorkflowActionModel Loading() =>
        new()
        {
            State = StudioWorkflowActionState.Loading,
            SafeMessage = "Loading review workflow."
        };

    public static StudioWorkflowActionModel FromContent(StudioContentListItem? content)
    {
        if (content?.CurrentVersion is null)
        {
            return new StudioWorkflowActionModel
            {
                State = StudioWorkflowActionState.Error,
                SafeMessage = "Review workflow could not be loaded."
            };
        }

        return new StudioWorkflowActionModel
        {
            ContentId = content.Id,
            VersionId = content.CurrentVersion.Id,
            LifecycleState = content.LifecycleState,
            State = StudioWorkflowActionState.Ready,
            SafeMessage = GetReadinessMessage(content.LifecycleState)
        };
    }

    public static IReadOnlyCollection<StudioWorkflowActionKind> GetAvailableActions(
        StudioContentLifecycleState lifecycleState) =>
        lifecycleState switch
        {
            StudioContentLifecycleState.Draft => [StudioWorkflowActionKind.RequestReview],
            StudioContentLifecycleState.InReview =>
            [
                StudioWorkflowActionKind.Approve,
                StudioWorkflowActionKind.Reject
            ],
            _ => []
        };

    public static bool IsActionAvailable(
        StudioContentLifecycleState lifecycleState,
        StudioWorkflowActionKind action) =>
        GetAvailableActions(lifecycleState).Contains(action);

    public static string GetActionLabel(StudioWorkflowActionKind action) =>
        action switch
        {
            StudioWorkflowActionKind.RequestReview => "Request review",
            StudioWorkflowActionKind.Approve => "Approve",
            StudioWorkflowActionKind.Reject => "Return to draft",
            _ => "Unsupported"
        };

    public static string GetRequiredPermission(StudioWorkflowActionKind action) =>
        action switch
        {
            StudioWorkflowActionKind.Approve => AdoptaPermissionKeys.AuthoringApprove,
            StudioWorkflowActionKind.RequestReview => AdoptaPermissionKeys.AuthoringReview,
            StudioWorkflowActionKind.Reject => AdoptaPermissionKeys.AuthoringReview,
            _ => AdoptaPermissionKeys.AuthoringRead
        };

    public static StudioContentLifecycleState GetTargetLifecycleState(
        StudioWorkflowActionKind action) =>
        action switch
        {
            StudioWorkflowActionKind.RequestReview => StudioContentLifecycleState.InReview,
            StudioWorkflowActionKind.Approve => StudioContentLifecycleState.Approved,
            StudioWorkflowActionKind.Reject => StudioContentLifecycleState.Draft,
            _ => StudioContentLifecycleState.Draft
        };

    public static string GetReadinessMessage(StudioContentLifecycleState lifecycleState) =>
        lifecycleState switch
        {
            StudioContentLifecycleState.Draft => "Draft content can be submitted for review.",
            StudioContentLifecycleState.InReview => "Content is ready for review decision.",
            StudioContentLifecycleState.Approved => "Content is approved. Publishing remains out of scope for this view.",
            StudioContentLifecycleState.Published => "Published content has no review workflow actions in this view.",
            StudioContentLifecycleState.Archived => "Archived content has no review workflow actions in this view.",
            _ => "Review workflow is ready."
        };

    public StudioWorkflowActionValidationResult Validate(StudioWorkflowActionKind action)
    {
        var issues = new List<StudioWorkflowActionIssue>();

        if (ContentId == Guid.Empty)
        {
            issues.Add(Issue("required", "contentId", "Content selection is required."));
        }

        if (VersionId == Guid.Empty)
        {
            issues.Add(Issue("required", "versionId", "Version selection is required."));
        }

        if (!IsActionAvailable(LifecycleState, action))
        {
            issues.Add(Issue(
                "invalid_lifecycle_transition",
                "lifecycleState",
                "Workflow action is not available for the selected lifecycle state."));
        }

        return issues.Count == 0
            ? StudioWorkflowActionValidationResult.Success()
            : StudioWorkflowActionValidationResult.Failure(issues);
    }

    public void ApplyValidationResult(StudioWorkflowActionValidationResult validation)
    {
        Issues = validation.Issues;
        State = validation.Succeeded
            ? StudioWorkflowActionState.Ready
            : StudioWorkflowActionState.ValidationError;
        SafeMessage = validation.Succeeded
            ? GetReadinessMessage(LifecycleState)
            : "Review workflow has validation issues.";
    }

    public void MarkProcessing(StudioWorkflowActionKind action)
    {
        Issues = [];
        LastAction = action;
        State = StudioWorkflowActionState.Processing;
        SafeMessage = "Review workflow action is processing.";
    }

    public void MarkSucceeded(StudioWorkflowActionKind action)
    {
        Issues = [];
        LastAction = action;
        LifecycleState = GetTargetLifecycleState(action);
        State = StudioWorkflowActionState.Succeeded;
        SafeMessage = action switch
        {
            StudioWorkflowActionKind.RequestReview => "Review requested locally.",
            StudioWorkflowActionKind.Approve => "Content approved locally.",
            StudioWorkflowActionKind.Reject => "Content returned to draft locally.",
            _ => "Review workflow action completed locally."
        };
    }

    public void MarkNotAuthorized(string safeMessage)
    {
        Issues = [];
        State = StudioWorkflowActionState.NotAuthorized;
        SafeMessage = safeMessage;
    }

    public void MarkError(string safeMessage)
    {
        Issues = [];
        State = StudioWorkflowActionState.Error;
        SafeMessage = safeMessage;
    }

    private static StudioWorkflowActionIssue Issue(
        string code,
        string path,
        string message) =>
        new(code, path, message);
}
