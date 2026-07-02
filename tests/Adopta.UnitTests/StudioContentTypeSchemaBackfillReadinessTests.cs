namespace Adopta.UnitTests;

public sealed class StudioContentTypeSchemaBackfillReadinessTests
{
    private const string ReadinessPath =
        "docs/adopta/studio/STUDIO-CONTENT-TYPE-SCHEMA-BACKFILL-READINESS.md";

    private const string DesignPath =
        "docs/adopta/studio/STUDIO-CONTENT-TYPE-SOURCE-OF-TRUTH-DESIGN.md";

    private const string Sprint11Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-11.md";

    private const string ReadmePath =
        "docs/adopta/README.md";

    private const string MigrationDirectory =
        "src/Adopta.Infrastructure/Persistence/Migrations";

    [Fact]
    public void Content_type_schema_backfill_readiness_guide_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, ReadinessPath)));
    }

    [Fact]
    public void Content_type_migration_source_exists_and_is_documented_as_review_only()
    {
        var repository = FindRepositoryRoot();
        var migrationPath = Directory.EnumerateFiles(
                Path.Combine(repository.FullName, MigrationDirectory),
                "*_AddAuthoredContentType.cs")
            .Single(file => !file.EndsWith(".Designer.cs", StringComparison.Ordinal));
        var readiness = ReadRepositoryFile(ReadinessPath);

        Assert.EndsWith("20260702000100_AddAuthoredContentType.cs", migrationPath, StringComparison.Ordinal);
        Assert.Contains("20260702000100_AddAuthoredContentType.cs", readiness, StringComparison.Ordinal);
        Assert.Contains("review-only migration source", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Migration execution is not approved", readiness, StringComparison.Ordinal);
    }

    [Fact]
    public void Readiness_docs_require_operational_approval_and_forbid_startup_database_mutation()
    {
        var docs = ReadReadinessDocs();

        Assert.Contains("explicit operational approval", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("approval-gated", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("outside normal application startup", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no database is created automatically", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no startup database mutation", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("automatic migration execution", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("schema mutation in this slice", docs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Readiness_docs_cover_backfill_rollback_and_fail_closed_behaviour()
    {
        var readiness = ReadRepositoryFile(ReadinessPath);

        Assert.Contains("Backfill Strategy", readiness, StringComparison.Ordinal);
        Assert.Contains("tenant-scoped authored content inventory", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must not be inferred", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Rollback And Fail-Closed Expectations", readiness, StringComparison.Ordinal);
        Assert.Contains("rollback requires manual approval", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fail-closed", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("missing or invalid content type must not be treated as authoritative", readiness, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Readiness_docs_cover_pre_and_post_execution_validation_checks()
    {
        var readiness = ReadRepositoryFile(ReadinessPath);

        Assert.Contains("Pre-Execution Checks", readiness, StringComparison.Ordinal);
        Assert.Contains("Post-Execution Validation Checks", readiness, StringComparison.Ordinal);
        Assert.Contains("new authored content requires a valid content type", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant-scoped reads remain tenant isolated", readiness, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no content body or sensitive values are exposed", readiness, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Content_type_remains_required_for_new_content()
    {
        var validatorSource = ReadRepositoryFile("src/Adopta.Application/Authoring/AuthoredContentValidator.cs");
        var readiness = ReadRepositoryFile(ReadinessPath);

        Assert.Contains("RequireContentType", validatorSource, StringComparison.Ordinal);
        Assert.Contains("content.contentType", validatorSource, StringComparison.Ordinal);
        Assert.Contains("content type remains required for new content", readiness, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_11_and_readme_reference_closeout_and_readiness_material()
    {
        var sprint = ReadRepositoryFile(Sprint11Path);
        var design = ReadRepositoryFile(DesignPath);
        var readme = ReadRepositoryFile(ReadmePath);

        Assert.Contains("Slice 5 - Content type schema/backfill readiness and Sprint 11 closeout", sprint, StringComparison.Ordinal);
        Assert.Contains("Sprint 11 closeout status", sprint, StringComparison.Ordinal);
        Assert.Contains("STUDIO-CONTENT-TYPE-SCHEMA-BACKFILL-READINESS.md", design, StringComparison.Ordinal);
        Assert.Contains("Studio Content Type Schema And Backfill Readiness", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Readiness_docs_keep_live_write_workflow_publish_out_of_scope()
    {
        var docs = ReadReadinessDocs();

        Assert.Contains("live create/update/review/approve/reject/publish integration", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live write/workflow/publish integration remains unavailable", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live read activation by default", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Property MTD integration", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Readiness_docs_do_not_contain_obvious_secret_markers()
    {
        var docs = string.Concat(
            ReadReadinessDocs(),
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

    private static string ReadReadinessDocs() =>
        string.Concat(
            ReadRepositoryFile(ReadinessPath),
            Environment.NewLine,
            ReadRepositoryFile(DesignPath),
            Environment.NewLine,
            ReadRepositoryFile(Sprint11Path));

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
