namespace Adopta.UnitTests;

public sealed class StudioContentTypeSourceOfTruthPlanningTests
{
    private const string DesignPath =
        "docs/adopta/studio/STUDIO-CONTENT-TYPE-SOURCE-OF-TRUTH-DESIGN.md";

    private const string Sprint11Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-11.md";

    [Fact]
    public void Content_type_source_of_truth_design_doc_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, DesignPath)));
    }

    [Fact]
    public void Design_doc_identifies_item_level_source_of_truth()
    {
        var design = ReadRepositoryFile(DesignPath);

        Assert.Contains("Content type must be a real source-of-truth field", design, StringComparison.Ordinal);
        Assert.Contains("must live on the authored content item", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not on the authored content version", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("domain-owned enum", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Tooltip", design, StringComparison.Ordinal);
        Assert.Contains("Callout", design, StringComparison.Ordinal);
        Assert.Contains("Checklist", design, StringComparison.Ordinal);
        Assert.Contains("Walkthrough", design, StringComparison.Ordinal);
    }

    [Fact]
    public void Design_doc_rejects_inference_and_fake_mapper_accuracy()
    {
        var design = ReadRepositoryFile(DesignPath);

        Assert.Contains("must not be inferred", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("content key", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("title", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("route", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("selector", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UI fallback", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must not fake content type accuracy", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HasKnownContentType=true", design, StringComparison.Ordinal);
        Assert.Contains("only when the live authoring API returns", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HasKnownContentType=false", design, StringComparison.Ordinal);
    }

    [Fact]
    public void Design_doc_covers_immutability_and_validation_rules()
    {
        var design = ReadRepositoryFile(DesignPath);

        Assert.Contains("required for new live authored content", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("immutable after live authoring creation", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("future separately approved rule allows draft-only mutation", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("one of the controlled values", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validation messages do not echo raw input values", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Unknown", design, StringComparison.Ordinal);
        Assert.Contains("not be a normal valid value", design, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Design_doc_covers_backward_compatibility_and_schema_planning()
    {
        var design = ReadRepositoryFile(DesignPath);

        Assert.Contains("Existing content without content type must remain unknown or unavailable", design, StringComparison.Ordinal);
        Assert.Contains("ContentType", design, StringComparison.Ordinal);
        Assert.Contains("AuthoredContentItems", design, StringComparison.Ordinal);
        Assert.Contains("nvarchar(32)", design, StringComparison.Ordinal);
        Assert.Contains("Migration and backfill must be separately approved", design, StringComparison.Ordinal);
        Assert.Contains("No migration should execute automatically", design, StringComparison.Ordinal);
        Assert.Contains("nullable or staged migration", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("controlled backfill", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validation requiring type for new content only", design, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Design_doc_covers_future_implementation_impacts_and_non_goals()
    {
        var design = ReadRepositoryFile(DesignPath);

        Assert.Contains("Domain Model", design, StringComparison.Ordinal);
        Assert.Contains("API Contract Impact", design, StringComparison.Ordinal);
        Assert.Contains("Runtime Bundle Mapping Impact", design, StringComparison.Ordinal);
        Assert.Contains("Web And Studio Impact", design, StringComparison.Ordinal);
        Assert.Contains("Future Implementation Impact", design, StringComparison.Ordinal);
        Assert.Contains("live create/update/review/approve/reject/publish integration", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("review-only EF migration source", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("migration execution", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("startup database mutation", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live read activation by default", design, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Design_doc_covers_security_guardrails()
    {
        var design = ReadRepositoryFile(DesignPath);

        Assert.Contains("no tenant IDs from browser, page, or request models", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("X-Adopta-Tenant-Id", design, StringComparison.Ordinal);
        Assert.Contains("X-Adopta-Test-*", design, StringComparison.Ordinal);
        Assert.Contains("Authoring.Read", design, StringComparison.Ordinal);
        Assert.Contains("Authoring.Manage", design, StringComparison.Ordinal);
        Assert.Contains("cross-tenant existence remains hidden", design, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_11_document_references_slice_3_planning()
    {
        var sprint = ReadRepositoryFile(Sprint11Path);

        Assert.Contains("Slice 3 - Content type source-of-truth planning", sprint, StringComparison.Ordinal);
        Assert.Contains("STUDIO-CONTENT-TYPE-SOURCE-OF-TRUTH-DESIGN.md", sprint, StringComparison.Ordinal);
        Assert.Contains("authored content item", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not the version", sprint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Design_docs_do_not_contain_obvious_secret_markers()
    {
        var docs = string.Concat(
            ReadRepositoryFile(DesignPath),
            Environment.NewLine,
            ReadRepositoryFile(Sprint11Path));

        Assert.DoesNotContain(Forbidden("Password", "="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("User", " Id="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Account", "Key="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Bearer", " "), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Client", "Secret"), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=tcp:", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Initial Catalog=", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Data Source=", docs, StringComparison.OrdinalIgnoreCase);
    }

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
