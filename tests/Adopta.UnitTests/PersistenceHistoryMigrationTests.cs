namespace Adopta.UnitTests;

public sealed class PersistenceHistoryMigrationTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void History_persistence_migration_exists()
    {
        var migrationDirectory = MigrationDirectory();

        Assert.Contains(Directory.EnumerateFiles(migrationDirectory), file =>
            file.EndsWith("_AddAuthoringHistoryPersistence.cs", StringComparison.Ordinal));
        Assert.Contains(Directory.EnumerateFiles(migrationDirectory), file =>
            file.EndsWith("_AddAuthoringHistoryPersistence.Designer.cs", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("AuthoredContentLifecycleHistory")]
    [InlineData("AuthoredContentPublishingHistory")]
    public void History_persistence_migration_contains_expected_tables(string tableName)
    {
        var migration = ReadHistoryMigration();

        Assert.Contains($"name: \"{tableName}\"", migration, StringComparison.Ordinal);
        Assert.Contains("schema: \"adopta\"", migration, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("AuthoredContentLifecycleHistory")]
    [InlineData("AuthoredContentPublishingHistory")]
    public void History_persistence_migration_contains_tenant_ownership_columns(string tableName)
    {
        var tableBlock = ExtractTableBlock(ReadHistoryMigration(), tableName);

        Assert.Contains("TenantId = table.Column<Guid>", tableBlock, StringComparison.Ordinal);
        Assert.Contains("nullable: false", tableBlock, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("IX_AuthoredContentLifecycleHistory_TenantId_OccurredAtUtc")]
    [InlineData("IX_AuthoredContentLifecycleHistory_TenantId_ContentId_VersionId")]
    [InlineData("IX_AuthoredContentPublishingHistory_TenantId_OccurredAtUtc")]
    [InlineData("IX_AuthoredContentPublishingHistory_TenantId_ContentId_VersionId")]
    [InlineData("IX_AuthoredContentPublishingHistory_TenantId_Environment_Channel")]
    public void History_persistence_migration_contains_tenant_scoped_indexes(string indexName)
    {
        var migration = ReadHistoryMigration();

        Assert.Contains($"name: \"{indexName}\"", migration, StringComparison.Ordinal);
    }

    private static string ReadHistoryMigration()
    {
        var migrationFile = Directory.EnumerateFiles(MigrationDirectory(), "*_AddAuthoringHistoryPersistence.cs")
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
