using Adopta.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace Adopta.UnitTests;

public sealed class PersistenceReadinessContractTests
{
    [Fact]
    public void Disabled_persistence_readiness_is_non_leaky_disabled_result()
    {
        var configuration = new ConfigurationBuilder().Build();
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.EvaluateReadiness(options, configuration);

        Assert.Equal(AdoptaPersistenceReadinessStatus.Disabled, result.Status);
        Assert.False(result.IsUsableForEfRegistration);
        Assert.Null(result.Provider);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Invalid_configuration_readiness_contains_safe_typed_issues()
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: "",
            connectionStringName: "",
            connectionStringValue: null);
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.EvaluateReadiness(options, configuration);

        Assert.Equal(AdoptaPersistenceReadinessStatus.InvalidConfiguration, result.Status);
        Assert.False(result.IsUsableForEfRegistration);
        Assert.Null(result.Provider);
        Assert.NotEmpty(result.Issues);
        Assert.All(result.Issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Path));
            Assert.False(string.IsNullOrWhiteSpace(issue.Message));
        });
    }

    [Fact]
    public void Valid_enabled_sql_server_readiness_is_configured_but_not_connectivity_checked()
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: AdoptaPersistenceOptions.SqlServerProvider,
            connectionStringName: "AdoptaSqlServer",
            connectionStringValue: "__configured_by_secure_provider__");
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.EvaluateReadiness(options, configuration);

        Assert.Equal(AdoptaPersistenceReadinessStatus.ConfiguredConnectivityNotChecked, result.Status);
        Assert.True(result.IsUsableForEfRegistration);
        Assert.Equal(AdoptaPersistenceOptions.SqlServerProvider, result.Provider);
        Assert.Empty(result.Issues);
    }

    private static IConfiguration BuildConfiguration(
        bool enabled,
        string? provider,
        string? connectionStringName,
        string? connectionStringValue)
    {
        var values = new Dictionary<string, string?>
        {
            ["Persistence:Enabled"] = enabled.ToString(),
            ["Persistence:Provider"] = provider,
            ["Persistence:SqlServer:ConnectionStringName"] = connectionStringName
        };

        if (!string.IsNullOrEmpty(connectionStringName) && connectionStringValue is not null)
        {
            values[$"ConnectionStrings:{connectionStringName}"] = connectionStringValue;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
