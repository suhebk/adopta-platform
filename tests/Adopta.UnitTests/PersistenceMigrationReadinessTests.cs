using System.Text.Json;

namespace Adopta.UnitTests;

public sealed class PersistenceMigrationReadinessTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Theory]
    [InlineData("Migrate(")]
    [InlineData("EnsureCreated(")]
    [InlineData("EnsureDeleted(")]
    [InlineData("Database.Ensure")]
    public void Source_does_not_execute_migrations_or_create_databases(string forbiddenPattern)
    {
        var files = EnumerateFiles("src", "tests")
            .Where(file => !IsApprovedMigrationSource(file));

        Assert.DoesNotContain(files, file =>
            File.ReadAllText(file).Contains(forbiddenPattern, StringComparison.Ordinal));
    }

    [Fact]
    public void Ef_design_package_is_tooling_only_when_referenced()
    {
        var projectFiles = EnumerateFiles("src", "tests")
            .Where(file => file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

        var references = projectFiles
            .Select(file => new
            {
                File = file,
                Content = File.ReadAllText(file)
            })
            .Where(project => project.Content.Contains("Microsoft.EntityFrameworkCore.Design", StringComparison.Ordinal))
            .ToArray();

        var infrastructureProject = Path.Combine(
            RepositoryRoot,
            "src",
            "Adopta.Infrastructure",
            "Adopta.Infrastructure.csproj");

        var reference = Assert.Single(references);
        Assert.Equal(infrastructureProject, reference.File);
        Assert.Contains("PrivateAssets>all</PrivateAssets", reference.Content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("src/Adopta.Api/appsettings.json")]
    [InlineData("src/Adopta.Api/appsettings.Development.json")]
    public void Appsettings_keep_persistence_disabled_by_default(string relativePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryRoot, relativePath)));
        var persistence = document.RootElement.GetProperty("Persistence");

        Assert.False(persistence.GetProperty("Enabled").GetBoolean());
        Assert.Equal("", persistence.GetProperty("Provider").GetString());
    }

    [Theory]
    [InlineData("src/Adopta.Api/appsettings.json")]
    [InlineData("src/Adopta.Api/appsettings.Development.json")]
    public void Appsettings_do_not_include_real_connection_strings(string relativePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryRoot, relativePath)));

        Assert.False(document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            && connectionStrings.EnumerateObject().Any(property => !string.IsNullOrWhiteSpace(property.Value.GetString())));
    }

    [Theory]
    [InlineData("docs/adopta/persistence/MIGRATION-STRATEGY.md")]
    [InlineData("docs/adopta/persistence/TENANT-ISOLATION-DATABASE-BOUNDARY.md")]
    [InlineData("docs/adopta/persistence/PERSISTENCE-OPERATIONS-RUNBOOK.md")]
    public void Persistence_docs_include_required_operational_guidance(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(RepositoryRoot, relativePath));

        Assert.Contains("rollback", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("approval", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("startup", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("migration", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Migration_strategy_documents_environment_promotion()
    {
        var content = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "docs/adopta/persistence/MIGRATION-STRATEGY.md"));

        Assert.Contains("Environment Promotion", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Dev", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("QA", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Production", content, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateFiles(params string[] relativeRoots)
    {
        return relativeRoots
            .Select(relativeRoot => Path.Combine(RepositoryRoot, relativeRoot))
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.EndsWith(nameof(PersistenceMigrationReadinessTests) + ".cs", StringComparison.Ordinal));
    }

    private static bool IsApprovedMigrationSource(string file)
    {
        return file.Contains(
            $"{Path.DirectorySeparatorChar}Persistence{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}",
            StringComparison.Ordinal);
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
