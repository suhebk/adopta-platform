namespace Adopta.Web.Studio;

public enum StudioReadApiActivationStatus
{
    Disabled = 0,
    Active = 1,
    Invalid = 2
}

public sealed record StudioReadApiActivationValidationIssue(
    string Code,
    string Message);

public sealed record StudioReadApiActivationValidationResult(
    StudioReadApiActivationStatus Status,
    IReadOnlyCollection<StudioReadApiActivationValidationIssue> Issues)
{
    public bool CanActivate => Status == StudioReadApiActivationStatus.Active;

    public static StudioReadApiActivationValidationResult Disabled() =>
        new(StudioReadApiActivationStatus.Disabled, []);

    public static StudioReadApiActivationValidationResult Active() =>
        new(StudioReadApiActivationStatus.Active, []);

    public static StudioReadApiActivationValidationResult Invalid(
        IReadOnlyCollection<StudioReadApiActivationValidationIssue> issues) =>
        new(StudioReadApiActivationStatus.Invalid, issues);
}
