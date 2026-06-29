namespace Adopta.Infrastructure.Persistence;

public sealed record AdoptaPersistenceValidationResult(
    IReadOnlyCollection<AdoptaPersistenceValidationIssue> Issues)
{
    public bool IsValid => Issues.Count == 0;

    public static AdoptaPersistenceValidationResult Valid { get; } = new(Array.Empty<AdoptaPersistenceValidationIssue>());

    public static AdoptaPersistenceValidationResult Invalid(
        params AdoptaPersistenceValidationIssue[] issues)
    {
        return new AdoptaPersistenceValidationResult(issues);
    }
}

public sealed record AdoptaPersistenceValidationIssue(
    AdoptaPersistenceValidationIssueCode Code,
    string Path,
    string Message);

public enum AdoptaPersistenceValidationIssueCode
{
    ProviderRequired,
    UnsupportedProvider,
    SqlServerConnectionStringNameRequired,
    SqlServerConnectionStringRequired
}
