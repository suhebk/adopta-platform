namespace Adopta.UnitTests;

public sealed class PersistenceSchemaBaselineTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void Initial_schema_baseline_migration_files_exist()
    {
        var migrationDirectory = MigrationDirectory();

        Assert.Contains(Directory.EnumerateFiles(migrationDirectory), file =>
            file.EndsWith("_InitialAdoptaSchemaBaseline.cs", StringComparison.Ordinal));
        Assert.Contains(Directory.EnumerateFiles(migrationDirectory), file =>
            file.EndsWith("_InitialAdoptaSchemaBaseline.Designer.cs", StringComparison.Ordinal));
        Assert.True(File.Exists(Path.Combine(migrationDirectory, "AdoptaDbContextModelSnapshot.cs")));
    }

    [Theory]
    [InlineData("Tenants")]
    [InlineData("TenantApplications")]
    [InlineData("AdoptionUsers")]
    [InlineData("Roles")]
    [InlineData("Permissions")]
    [InlineData("TenantMappings")]
    [InlineData("AuthenticatedUserMappings")]
    [InlineData("AuthoredContentItems")]
    [InlineData("AuthoredContentVersions")]
    [InlineData("AuditEvents")]
    [InlineData("SecurityAuditEvents")]
    public void Schema_baseline_contains_expected_tables(string tableName)
    {
        var migration = ReadInitialMigration();

        Assert.Contains($"name: \"{tableName}\"", migration, StringComparison.Ordinal);
        Assert.Contains("schema: \"adopta\"", migration, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("TenantApplications")]
    [InlineData("AdoptionUsers")]
    [InlineData("Roles")]
    [InlineData("TenantMappings")]
    [InlineData("AuthenticatedUserMappings")]
    [InlineData("AuthoredContentItems")]
    [InlineData("AuthoredContentVersions")]
    [InlineData("AuditEvents")]
    [InlineData("SecurityAuditEvents")]
    [InlineData("AdoptionUserRoles")]
    [InlineData("RolePermissions")]
    public void Tenant_scoped_tables_include_tenant_ownership(string tableName)
    {
        var migration = ReadInitialMigration();
        var tableBlock = ExtractTableBlock(migration, tableName);

        Assert.Contains("TenantId = table.Column<Guid>", tableBlock, StringComparison.Ordinal);
        Assert.Contains("nullable: false", tableBlock, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("IX_AuthoredContentItems_TenantId_ApplicationId_ContentKey")]
    [InlineData("IX_AdoptionUsers_TenantId_ExternalUserId")]
    [InlineData("IX_Roles_TenantId_Name")]
    [InlineData("IX_TenantMappings_TenantId_ExternalTenantId_ApplicationId")]
    [InlineData("IX_SecurityAuditEvents_TenantId_OccurredAtUtc")]
    public void Schema_baseline_contains_expected_tenant_scoped_indexes(string indexName)
    {
        var migration = ReadInitialMigration();

        Assert.Contains($"name: \"{indexName}\"", migration, StringComparison.Ordinal);
    }

    [Fact]
    public void Design_time_factory_uses_placeholder_only()
    {
        var content = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "src",
            "Adopta.Infrastructure",
            "Persistence",
            "DesignTimeAdoptaDbContextFactory.cs"));

        Assert.Contains("__adopta_design_time_placeholder__", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Server=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("User Id=", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Schema_baseline_review_document_exists_and_marks_execution_out_of_scope()
    {
        var content = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "docs",
            "adopta",
            "persistence",
            "SCHEMA-BASELINE-REVIEW.md"));

        Assert.Contains("tables included", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant-owned", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("approval", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("migration execution is not approved", content, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadInitialMigration()
    {
        var migrationFile = Directory.EnumerateFiles(MigrationDirectory(), "*_InitialAdoptaSchemaBaseline.cs")
            .Single(file => !file.EndsWith(".Designer.cs", StringComparison.Ordinal));

        return File.ReadAllText(migrationFile);
    }

    private static string ExtractTableBlock(string migration, string tableName)
    {
        var start = migration.IndexOf($"name: \"{tableName}\"", StringComparison.Ordinal);
        Assert.True(start >= 0);
        var nextTable = migration.IndexOf("migrationBuilder.CreateTable(", start + tableName.Length, StringComparison.Ordinal);
        return nextTable < 0
            ? migration[start..]
            : migration[start..nextTable];
    }

    private static string MigrationDirectory()
    {
        return Path.Combine(
            RepositoryRoot,
            "src",
            "Adopta.Infrastructure",
            "Persistence",
            "Migrations");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Adopta.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Repository root could not be resolved.");
    }
}
