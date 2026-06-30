namespace Adopta.Web.Studio;

public sealed record StudioWebAuthenticationValidationIssue(
    string Code,
    string Message);

public sealed record StudioWebAuthenticationValidationResult(
    bool IsValid,
    IReadOnlyCollection<StudioWebAuthenticationValidationIssue> Issues)
{
    public static StudioWebAuthenticationValidationResult Valid() =>
        new(true, []);

    public static StudioWebAuthenticationValidationResult Failed(
        IReadOnlyCollection<StudioWebAuthenticationValidationIssue> issues) =>
        new(false, issues);
}
