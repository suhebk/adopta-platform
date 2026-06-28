namespace Adopta.Application.Runtime;

public sealed record RuntimeContentValidationIssue(
    string Code,
    string Path,
    string Message);

