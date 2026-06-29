namespace Adopta.Infrastructure.Persistence;

public sealed record AdoptaPersistenceReadinessResult(
    AdoptaPersistenceReadinessStatus Status,
    string? Provider,
    IReadOnlyCollection<AdoptaPersistenceValidationIssue> Issues)
{
    public bool IsUsableForEfRegistration => Status == AdoptaPersistenceReadinessStatus.ConfiguredConnectivityNotChecked;

    public static AdoptaPersistenceReadinessResult Disabled()
    {
        return new AdoptaPersistenceReadinessResult(
            AdoptaPersistenceReadinessStatus.Disabled,
            null,
            Array.Empty<AdoptaPersistenceValidationIssue>());
    }

    public static AdoptaPersistenceReadinessResult Invalid(
        IReadOnlyCollection<AdoptaPersistenceValidationIssue> issues)
    {
        return new AdoptaPersistenceReadinessResult(
            AdoptaPersistenceReadinessStatus.InvalidConfiguration,
            null,
            issues);
    }

    public static AdoptaPersistenceReadinessResult ConfiguredConnectivityNotChecked()
    {
        return new AdoptaPersistenceReadinessResult(
            AdoptaPersistenceReadinessStatus.ConfiguredConnectivityNotChecked,
            AdoptaPersistenceOptions.SqlServerProvider,
            Array.Empty<AdoptaPersistenceValidationIssue>());
    }
}
