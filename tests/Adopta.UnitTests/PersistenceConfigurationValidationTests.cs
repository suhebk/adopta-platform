using Adopta.Infrastructure;
using Adopta.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.UnitTests;

public sealed class PersistenceConfigurationValidationTests
{
    private const string FakeSensitiveMarker = "FAKE_SENSITIVE_CONNECTION_VALUE_SHOULD_NOT_APPEAR";

    [Fact]
    public void Disabled_persistence_validates_successfully_by_default()
    {
        var configuration = new ConfigurationBuilder().Build();
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.Validate(options, configuration);

        Assert.False(options.Enabled);
        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Enabled_sql_server_persistence_validates_when_required_configuration_exists()
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: AdoptaPersistenceOptions.SqlServerProvider,
            connectionStringName: "AdoptaSqlServer",
            connectionStringValue: "__configured_by_secure_provider__");
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.Validate(options, configuration);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Theory]
    [InlineData(null, AdoptaPersistenceValidationIssueCode.ProviderRequired)]
    [InlineData("", AdoptaPersistenceValidationIssueCode.ProviderRequired)]
    [InlineData("Unsupported", AdoptaPersistenceValidationIssueCode.UnsupportedProvider)]
    public void Enabled_persistence_requires_sql_server_provider(
        string? provider,
        AdoptaPersistenceValidationIssueCode expectedCode)
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: provider,
            connectionStringName: "AdoptaSqlServer",
            connectionStringValue: "__configured_by_secure_provider__");
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.Validate(options, configuration);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == expectedCode);
    }

    [Fact]
    public void Enabled_sql_server_persistence_requires_connection_string_name()
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: AdoptaPersistenceOptions.SqlServerProvider,
            connectionStringName: "",
            connectionStringValue: "__configured_by_secure_provider__");
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.Validate(options, configuration);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue =>
            issue.Code == AdoptaPersistenceValidationIssueCode.SqlServerConnectionStringNameRequired);
    }

    [Fact]
    public void Enabled_sql_server_persistence_requires_configured_connection_string()
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: AdoptaPersistenceOptions.SqlServerProvider,
            connectionStringName: "AdoptaSqlServer",
            connectionStringValue: "");
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.Validate(options, configuration);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue =>
            issue.Code == AdoptaPersistenceValidationIssueCode.SqlServerConnectionStringRequired);
    }

    [Fact]
    public void Validation_issues_do_not_expose_sensitive_configuration_values()
    {
        var configuration = BuildConfiguration(
            enabled: true,
            provider: "Unsupported",
            connectionStringName: "AdoptaSqlServer",
            connectionStringValue: FakeSensitiveMarker);
        var options = AdoptaPersistenceConfigurationValidator.ReadOptions(configuration);

        var result = AdoptaPersistenceConfigurationValidator.Validate(options, configuration);
        var issueText = string.Join(" ", result.Issues.Select(issue =>
            $"{issue.Code} {issue.Path} {issue.Message}"));

        Assert.False(result.IsValid);
        Assert.DoesNotContain(FakeSensitiveMarker, issueText, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", issueText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Host", issueText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Invalid_enabled_persistence_fails_startup_with_safe_message()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(
            enabled: true,
            provider: "Unsupported",
            connectionStringName: "AdoptaSqlServer",
            connectionStringValue: FakeSensitiveMarker);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddAdoptaInfrastructure(configuration));

        Assert.Equal(
            AdoptaPersistenceConfigurationValidator.SafeConfigurationErrorMessage,
            exception.Message);
        Assert.DoesNotContain(FakeSensitiveMarker, exception.Message, StringComparison.Ordinal);
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
