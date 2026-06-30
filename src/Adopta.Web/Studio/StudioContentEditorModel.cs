using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public enum StudioContentEditorState
{
    Loading = 0,
    Editing = 1,
    Saving = 2,
    Saved = 3,
    ValidationError = 4,
    NotAuthorized = 5,
    Error = 6
}

public sealed record StudioContentEditorIssue(
    string Code,
    string Path,
    string Message);

public sealed record StudioContentEditorValidationResult(
    bool Succeeded,
    IReadOnlyCollection<StudioContentEditorIssue> Issues)
{
    public static StudioContentEditorValidationResult Success() => new(true, []);

    public static StudioContentEditorValidationResult Failure(
        IReadOnlyCollection<StudioContentEditorIssue> issues) =>
        new(false, issues);
}

public sealed class StudioContentEditorModel
{
    private static readonly string[] SensitiveMarkers =
    [
        "bearer",
        "claim",
        "connection string",
        "connectionstring",
        "header",
        "hmrc",
        "password",
        "property data",
        "secret",
        "tax",
        "token"
    ];

    public Guid? ContentId { get; set; }

    public Guid ApplicationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ContentKey { get; set; } = string.Empty;

    public RuntimeContentType? ContentType { get; set; }

    public StudioContentLifecycleState LifecycleState { get; set; } = StudioContentLifecycleState.Draft;

    public string Version { get; set; } = "0.1.0";

    public StudioContentEditorState State { get; set; } = StudioContentEditorState.Editing;

    public string SafeMessage { get; set; } = "Draft metadata is ready.";

    public IReadOnlyCollection<StudioContentEditorIssue> Issues { get; private set; } = [];

    public bool HasValidationIssues => Issues.Count > 0;

    public static StudioContentEditorModel Loading() =>
        new()
        {
            State = StudioContentEditorState.Loading,
            SafeMessage = "Loading draft metadata."
        };

    public static StudioContentEditorModel FromContent(StudioContentListItem? content)
    {
        if (content is null)
        {
            return new StudioContentEditorModel
            {
                State = StudioContentEditorState.Error,
                SafeMessage = "Draft metadata could not be loaded."
            };
        }

        return new StudioContentEditorModel
        {
            ContentId = content.Id,
            ApplicationId = content.ApplicationId,
            Title = content.Title,
            ContentKey = content.ContentKey,
            ContentType = content.ContentType,
            LifecycleState = content.LifecycleState,
            Version = content.CurrentVersion?.Version ?? "Unavailable",
            State = StudioContentEditorState.Editing,
            SafeMessage = content.LifecycleState == StudioContentLifecycleState.Draft
                ? "Draft metadata is ready."
                : "Only draft metadata can be saved in this foundation view."
        };
    }

    public static StudioContentEditorModel FromCreateRequest(
        StudioContentCreateDraftRequest request) =>
        new()
        {
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            ContentKey = request.ContentKey,
            ContentType = request.ContentType,
            LifecycleState = StudioContentLifecycleState.Draft,
            Version = request.Version,
            State = StudioContentEditorState.Editing
        };

    public static StudioContentEditorModel FromUpdateRequest(
        StudioContentUpdateDraftRequest request,
        StudioContentLifecycleState lifecycleState = StudioContentLifecycleState.Draft) =>
        new()
        {
            ContentId = request.ContentId,
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            ContentKey = request.ContentKey,
            ContentType = request.ContentType,
            LifecycleState = lifecycleState,
            Version = request.Version,
            State = StudioContentEditorState.Editing
        };

    public StudioContentCreateDraftRequest ToCreateRequest() =>
        new(ApplicationId, Title.Trim(), ContentKey.Trim(), ContentType, Version.Trim());

    public StudioContentUpdateDraftRequest ToUpdateRequest() =>
        new(ContentId ?? Guid.Empty, ApplicationId, Title.Trim(), ContentKey.Trim(), ContentType, Version.Trim());

    public StudioContentEditorValidationResult Validate()
    {
        var issues = new List<StudioContentEditorIssue>();

        if (ApplicationId == Guid.Empty)
        {
            issues.Add(Issue("application_required", "applicationId", "Application selection is required."));
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            issues.Add(Issue("required", "title", "Title is required."));
        }

        if (string.IsNullOrWhiteSpace(ContentKey))
        {
            issues.Add(Issue("required", "contentKey", "Content key is required."));
        }
        else if (!IsSafeContentKey(ContentKey))
        {
            issues.Add(Issue("invalid_format", "contentKey", "Content key must use a safe structural format."));
        }

        if (ContentType is null)
        {
            issues.Add(Issue("required", "contentType", "Content type is required."));
        }
        else if (!Enum.IsDefined(ContentType.Value))
        {
            issues.Add(Issue("invalid_content_type", "contentType", "Content type is not supported."));
        }

        if (ContainsSensitiveMarker(Title) ||
            ContainsSensitiveMarker(ContentKey) ||
            ContainsSensitiveMarker(Version))
        {
            issues.Add(Issue("unsafe_metadata", "metadata", "Studio metadata contains unsupported values."));
        }

        return issues.Count == 0
            ? StudioContentEditorValidationResult.Success()
            : StudioContentEditorValidationResult.Failure(issues);
    }

    public void ApplyValidationResult(StudioContentEditorValidationResult validation)
    {
        Issues = validation.Issues;
        State = validation.Succeeded
            ? StudioContentEditorState.Editing
            : StudioContentEditorState.ValidationError;
        SafeMessage = validation.Succeeded
            ? "Draft metadata is ready."
            : "Draft metadata has validation issues.";
    }

    public void MarkSaving()
    {
        Issues = [];
        State = StudioContentEditorState.Saving;
        SafeMessage = "Saving draft metadata.";
    }

    public void MarkSaved()
    {
        Issues = [];
        State = StudioContentEditorState.Saved;
        SafeMessage = "Draft metadata saved locally.";
    }

    public void MarkNotAuthorized(string safeMessage)
    {
        Issues = [];
        State = StudioContentEditorState.NotAuthorized;
        SafeMessage = safeMessage;
    }

    public void MarkError(string safeMessage)
    {
        Issues = [];
        State = StudioContentEditorState.Error;
        SafeMessage = safeMessage;
    }

    private static bool IsSafeContentKey(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length is 0 or > 128 || !char.IsAsciiLetterLower(trimmed[0]))
        {
            return false;
        }

        var previousWasSeparator = false;
        foreach (var character in trimmed)
        {
            var isSegmentCharacter = char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character);
            var isSeparator = character is '.' or '-';
            if (!isSegmentCharacter && !isSeparator)
            {
                return false;
            }

            if (isSeparator && previousWasSeparator)
            {
                return false;
            }

            previousWasSeparator = isSeparator;
        }

        return !previousWasSeparator;
    }

    private static bool ContainsSensitiveMarker(string value) =>
        SensitiveMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static StudioContentEditorIssue Issue(
        string code,
        string path,
        string message) =>
        new(code, path, message);
}
