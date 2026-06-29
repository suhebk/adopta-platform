using Adopta.Domain.Authoring;

namespace Adopta.Application.Authoring;

public sealed record AuthoredContentContract(
    Guid Id,
    Guid TenantId,
    Guid ApplicationId,
    string ContentKey,
    string Title,
    IReadOnlyCollection<AuthoredContentVersionContract> Versions);

public sealed record AuthoredContentVersionContract(
    Guid Id,
    string Version,
    ContentLifecycleState LifecycleState,
    DateTimeOffset CreatedAtUtc);

public sealed record AuthoredContentValidationIssue(
    string Code,
    string Path,
    string Message);

public sealed record AuthoredContentValidationResult(
    bool IsValid,
    IReadOnlyCollection<AuthoredContentValidationIssue> Issues)
{
    public static AuthoredContentValidationResult Success()
    {
        return new AuthoredContentValidationResult(true, []);
    }

    public static AuthoredContentValidationResult Failure(
        IReadOnlyCollection<AuthoredContentValidationIssue> issues)
    {
        return new AuthoredContentValidationResult(false, issues);
    }
}
