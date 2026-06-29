using Microsoft.Extensions.Configuration;

namespace Adopta.Infrastructure.Persistence;

public static class AdoptaPersistenceConfigurationValidator
{
    public const string SafeConfigurationErrorMessage =
        "Persistence is enabled but SQL Server persistence is not configured.";

    public static AdoptaPersistenceOptions ReadOptions(IConfiguration? configuration)
    {
        var options = new AdoptaPersistenceOptions();
        if (configuration is null)
        {
            return options;
        }

        var section = configuration.GetSection(AdoptaPersistenceOptions.SectionName);
        options.Enabled = bool.TryParse(section[nameof(AdoptaPersistenceOptions.Enabled)], out var enabled) && enabled;
        options.Provider = section[nameof(AdoptaPersistenceOptions.Provider)];
        options.SqlServer.ConnectionStringName =
            section.GetSection(nameof(AdoptaPersistenceOptions.SqlServer))[
                nameof(SqlServerPersistenceOptions.ConnectionStringName)];

        return options;
    }

    public static AdoptaPersistenceValidationResult Validate(
        AdoptaPersistenceOptions options,
        IConfiguration? configuration)
    {
        if (!options.Enabled)
        {
            return AdoptaPersistenceValidationResult.Valid;
        }

        var issues = new List<AdoptaPersistenceValidationIssue>();

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            issues.Add(CreateIssue(
                AdoptaPersistenceValidationIssueCode.ProviderRequired,
                "Persistence:Provider",
                "Persistence provider is required when persistence is enabled."));
        }
        else if (!string.Equals(
            options.Provider,
            AdoptaPersistenceOptions.SqlServerProvider,
            StringComparison.Ordinal))
        {
            issues.Add(CreateIssue(
                AdoptaPersistenceValidationIssueCode.UnsupportedProvider,
                "Persistence:Provider",
                "The configured persistence provider is not supported."));
        }

        if (string.IsNullOrWhiteSpace(options.SqlServer.ConnectionStringName))
        {
            issues.Add(CreateIssue(
                AdoptaPersistenceValidationIssueCode.SqlServerConnectionStringNameRequired,
                "Persistence:SqlServer:ConnectionStringName",
                "SQL Server persistence requires a configured connection string name."));
        }
        else if (configuration is null
            || string.IsNullOrWhiteSpace(configuration.GetConnectionString(options.SqlServer.ConnectionStringName)))
        {
            issues.Add(CreateIssue(
                AdoptaPersistenceValidationIssueCode.SqlServerConnectionStringRequired,
                "ConnectionStrings",
                "SQL Server persistence requires a configured connection string."));
        }

        return issues.Count == 0
            ? AdoptaPersistenceValidationResult.Valid
            : new AdoptaPersistenceValidationResult(issues);
    }

    public static AdoptaPersistenceReadinessResult EvaluateReadiness(
        AdoptaPersistenceOptions options,
        IConfiguration? configuration)
    {
        var validation = Validate(options, configuration);

        if (!options.Enabled)
        {
            return AdoptaPersistenceReadinessResult.Disabled();
        }

        return validation.IsValid
            ? AdoptaPersistenceReadinessResult.ConfiguredConnectivityNotChecked()
            : AdoptaPersistenceReadinessResult.Invalid(validation.Issues);
    }

    private static AdoptaPersistenceValidationIssue CreateIssue(
        AdoptaPersistenceValidationIssueCode code,
        string path,
        string message)
    {
        return new AdoptaPersistenceValidationIssue(code, path, message);
    }
}
