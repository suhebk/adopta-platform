using Adopta.Application.Audit;
using Adopta.Application.Identity;
using Adopta.Domain.Audit;
using Adopta.Domain.Authoring;
using Adopta.Domain.Identity;
using Adopta.Domain.Tenancy;
using Adopta.Infrastructure;
using Adopta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adopta.UnitTests;

public sealed class PersistenceModelConfigurationTests
{
    private const string SafeConfigurationError = "Persistence is enabled but SQL Server persistence is not configured.";

    [Fact]
    public void Ef_model_builds_without_connecting_to_database()
    {
        using var dbContext = CreateDbContext();

        var entityTypes = dbContext.Model.GetEntityTypes();

        Assert.NotEmpty(entityTypes);
        Assert.Contains(entityTypes, entityType => entityType.ClrType == typeof(Tenant));
    }

    [Fact]
    public void Ef_model_contains_expected_persistence_entities()
    {
        using var dbContext = CreateDbContext();

        AssertEntityExists<Tenant>(dbContext);
        AssertEntityExists<TenantApplication>(dbContext);
        AssertEntityExists<AdoptionUser>(dbContext);
        AssertEntityExists<Role>(dbContext);
        AssertEntityExists<Permission>(dbContext);
        AssertEntityExists<TenantMappingRecord>(dbContext);
        AssertEntityExists<AuthenticatedUserMappingEntity>(dbContext);
        AssertEntityExists<AuthoredContentItem>(dbContext);
        AssertEntityExists<AuditEvent>(dbContext);
        AssertEntityExists<SecurityAuditEventRecord>(dbContext);

        Assert.Contains(dbContext.Model.GetEntityTypes(), entityType =>
            entityType.ClrType == typeof(AuthoredContentVersion) && entityType.IsOwned());
    }

    [Fact]
    public void Tenant_scoped_entities_expose_required_tenant_ownership()
    {
        using var dbContext = CreateDbContext();

        AssertRequiredTenantProperty<TenantApplication>(dbContext);
        AssertRequiredTenantProperty<AdoptionUser>(dbContext);
        AssertRequiredTenantProperty<Role>(dbContext);
        AssertRequiredTenantProperty<TenantMappingRecord>(dbContext);
        AssertRequiredTenantProperty<AuthenticatedUserMappingEntity>(dbContext);
        AssertRequiredTenantProperty<AuthoredContentItem>(dbContext);
        AssertRequiredTenantProperty<AuditEvent>(dbContext);
        AssertRequiredTenantProperty<SecurityAuditEventRecord>(dbContext);

        var authoredVersion = dbContext.Model.GetEntityTypes()
            .Single(entityType => entityType.ClrType == typeof(AuthoredContentVersion));
        var tenantId = authoredVersion.FindProperty("TenantId");

        Assert.NotNull(tenantId);
        Assert.False(tenantId.IsNullable);
    }

    [Fact]
    public void Important_keys_indexes_and_required_properties_are_configured()
    {
        using var dbContext = CreateDbContext();

        var tenantApplication = AssertEntityExists<TenantApplication>(dbContext);
        AssertHasIndex(tenantApplication, nameof(TenantApplication.TenantId), nameof(TenantApplication.Id));

        var authoredContent = AssertEntityExists<AuthoredContentItem>(dbContext);
        AssertHasIndex(
            authoredContent,
            nameof(AuthoredContentItem.TenantId),
            nameof(AuthoredContentItem.ApplicationId),
            nameof(AuthoredContentItem.ContentKey));
        Assert.False(authoredContent.FindProperty(nameof(AuthoredContentItem.Title))!.IsNullable);

        var tenantMapping = AssertEntityExists<TenantMappingRecord>(dbContext);
        Assert.NotNull(tenantMapping.FindPrimaryKey());
        Assert.False(tenantMapping.FindProperty(nameof(TenantMappingRecord.ExternalTenantId))!.IsNullable);

        var auditEvent = AssertEntityExists<AuditEvent>(dbContext);
        AssertHasIndex(auditEvent, nameof(AuditEvent.TenantId), nameof(AuditEvent.OccurredAtUtc));
    }

    [Fact]
    public void Persistence_is_disabled_by_default_and_does_not_register_db_context()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();

        services.AddAdoptaInfrastructure(configuration);

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(AdoptaDbContext));
    }

    [Theory]
    [InlineData("", "AdoptaSqlServer")]
    [InlineData("SqlServer", "")]
    [InlineData("SqlServer", "AdoptaSqlServer")]
    public void Persistence_configuration_fails_safely_when_sql_server_settings_are_missing(
        string provider,
        string connectionStringName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Enabled"] = "true",
                ["Persistence:Provider"] = provider,
                ["Persistence:SqlServer:ConnectionStringName"] = connectionStringName
            })
            .Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddAdoptaInfrastructure(configuration));

        Assert.Equal(SafeConfigurationError, exception.Message);
        Assert.DoesNotContain("ConnectionString", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AdoptaSqlServer", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AdoptaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AdoptaDbContext>()
            .UseSqlServer("Server=(local);Database=AdoptaModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AdoptaDbContext(options);
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType AssertEntityExists<TEntity>(
        DbContext dbContext)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

        Assert.NotNull(entityType);
        return entityType;
    }

    private static void AssertRequiredTenantProperty<TEntity>(DbContext dbContext)
    {
        var entityType = AssertEntityExists<TEntity>(dbContext);
        var tenantId = entityType.FindProperty("TenantId");

        Assert.NotNull(tenantId);
        Assert.False(tenantId.IsNullable);
    }

    private static void AssertHasIndex(
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
        params string[] propertyNames)
    {
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(propertyNames));
    }
}
