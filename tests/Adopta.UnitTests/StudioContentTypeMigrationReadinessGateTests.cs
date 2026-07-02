namespace Adopta.UnitTests;

public sealed class StudioContentTypeMigrationReadinessGateTests
{
    private const string Sprint12Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-12.md";

    private const string GatePath =
        "docs/adopta/studio/STUDIO-CONTENT-TYPE-MIGRATION-LIVE-DRAFT-READINESS-GATE.md";

    private const string SchemaReadinessPath =
        "docs/adopta/studio/STUDIO-CONTENT-TYPE-SCHEMA-BACKFILL-READINESS.md";

    private const string ReadmePath =
        "docs/adopta/README.md";

    [Fact]
    public void Sprint_12_document_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, Sprint12Path)));
    }

    [Fact]
    public void Readiness_gate_guide_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, GatePath)));
    }

    [Fact]
    public void Review_only_content_type_migration_source_exists()
    {
        var repository = FindRepositoryRoot();
        var migrationFile = Directory.EnumerateFiles(
                Path.Combine(
                    repository.FullName,
                    "src",
                    "Adopta.Infrastructure",
                    "Persistence",
                    "Migrations"),
                "*_AddAuthoredContentType.cs")
            .Single(file => !file.EndsWith(".Designer.cs", StringComparison.Ordinal));
        var migration = File.ReadAllText(migrationFile);
        var gate = ReadRepositoryFile(GatePath);

        Assert.EndsWith("20260702000100_AddAuthoredContentType.cs", migrationFile, StringComparison.Ordinal);
        Assert.Contains("name: \"ContentType\"", migration, StringComparison.Ordinal);
        Assert.Contains("table: \"AuthoredContentItems\"", migration, StringComparison.Ordinal);
        Assert.Contains("20260702000100_AddAuthoredContentType.cs", gate, StringComparison.Ordinal);
        Assert.Contains("review-only", gate, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ef_mapping_and_validation_still_require_content_type()
    {
        var configuration = ReadRepositoryFile(
            "src/Adopta.Infrastructure/Persistence/Configurations/AuthoredContentItemConfiguration.cs");
        var validator = ReadRepositoryFile("src/Adopta.Application/Authoring/AuthoredContentValidator.cs");

        Assert.Contains("content.ContentType", configuration, StringComparison.Ordinal);
        Assert.Contains("HasConversion<string>()", configuration, StringComparison.Ordinal);
        Assert.Contains("HasMaxLength(32)", configuration, StringComparison.Ordinal);
        Assert.Contains("IsRequired()", configuration, StringComparison.Ordinal);
        Assert.Contains("RequireContentType", validator, StringComparison.Ordinal);
        Assert.Contains("content.contentType", validator, StringComparison.Ordinal);
    }

    [Fact]
    public void Docs_block_live_draft_create_update_until_migration_backfill_readiness_is_approved()
    {
        var docs = ReadGateDocs();

        Assert.Contains("Live draft create/update must not proceed until content type migration/backfill readiness is approved", docs, StringComparison.Ordinal);
        Assert.Contains("live draft create/update is blocked", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backfill plan is approved", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback plan is approved", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("content type remains required for new content", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unknown/unavailable", docs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Docs_state_no_execution_mutation_startup_migration_or_deployment_automation()
    {
        var docs = ReadGateDocs();

        Assert.Contains("migration execution", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("database mutation", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("startup migration", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("automatic database creation", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("deployment automation", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No appsettings changes are required", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Docs_keep_live_workflow_publish_and_property_mtd_out_of_scope()
    {
        var docs = ReadGateDocs();

        Assert.Contains("live review/approve/reject/publish integration", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live workflow and publish wiring remains out of scope", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Property MTD integration", docs, StringComparison.Ordinal);
        Assert.Contains("no new permission keys", docs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Docs_reject_content_type_inference()
    {
        var gate = ReadRepositoryFile(GatePath);

        Assert.Contains("Content type must not be inferred", gate, StringComparison.Ordinal);
        Assert.Contains("content key", gate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("title", gate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("route", gate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("selector", gate, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UI fallback", gate, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Web_studio_read_client_remains_read_only()
    {
        var client = ReadRepositoryFile("src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs");

        Assert.DoesNotContain("PostAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PutAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PatchAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("DeleteAsync", client, StringComparison.Ordinal);
    }

    [Fact]
    public void Readme_references_sprint_12_and_readiness_gate()
    {
        var readme = ReadRepositoryFile(ReadmePath);

        Assert.Contains("ADOPTA-SPRINT-12", readme, StringComparison.Ordinal);
        Assert.Contains("Studio Content Type Migration And Live Draft Readiness Gate", readme, StringComparison.Ordinal);
        Assert.Contains("Current sprint: `ADOPTA-SPRINT-12 - Live Draft Authoring Readiness`", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Gate_docs_do_not_contain_obvious_secret_markers()
    {
        var docs = string.Concat(
            ReadGateDocs(),
            Environment.NewLine,
            ReadRepositoryFile(ReadmePath));

        Assert.DoesNotContain(Forbidden("Password", "="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("User", " Id="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Account", "Key="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Bearer", " "), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Client", "Secret"), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=tcp:", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Initial Catalog=", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Data Source=", docs, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadGateDocs() =>
        string.Concat(
            ReadRepositoryFile(GatePath),
            Environment.NewLine,
            ReadRepositoryFile(SchemaReadinessPath),
            Environment.NewLine,
            ReadRepositoryFile(Sprint12Path));

    private static string Forbidden(string left, string right) =>
        string.Concat(left, right);

    private static string ReadRepositoryFile(string relativePath)
    {
        var repository = FindRepositoryRoot();

        return File.ReadAllText(Path.Combine(repository.FullName, relativePath));
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Adopta.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return directory;
    }
}
