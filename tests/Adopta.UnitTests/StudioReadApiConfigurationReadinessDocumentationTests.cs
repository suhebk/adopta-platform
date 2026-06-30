namespace Adopta.UnitTests;

public sealed class StudioReadApiConfigurationReadinessDocumentationTests
{
    private const string ReadinessGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ACTIVATION-READINESS.md";

    private const string Sprint9Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-9.md";

    [Fact]
    public void Studio_read_api_activation_readiness_guide_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, ReadinessGuidePath)));
    }

    [Fact]
    public void Readiness_guide_documents_required_configuration_keys()
    {
        var guide = ReadRepositoryFile(ReadinessGuidePath);

        Assert.Contains("StudioApi:Enabled", guide, StringComparison.Ordinal);
        Assert.Contains("StudioApi:BaseAddress", guide, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:Enabled", guide, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:Authority", guide, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:ClientId", guide, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:CallbackPath", guide, StringComparison.Ordinal);
        Assert.Contains("StudioApi:TokenAcquisition:Enabled", guide, StringComparison.Ordinal);
        Assert.Contains("StudioApi:TokenAcquisition:Scopes", guide, StringComparison.Ordinal);
    }

    [Fact]
    public void Readiness_guide_documents_activation_prerequisites()
    {
        var guide = ReadRepositoryFile(ReadinessGuidePath);

        Assert.Contains("StudioApi:Enabled=true", guide, StringComparison.Ordinal);
        Assert.Contains("HTTPS absolute base address", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Authentication:StudioWeb:Enabled=true", guide, StringComparison.Ordinal);
        Assert.Contains("StudioApi:TokenAcquisition:Enabled=true", guide, StringComparison.Ordinal);
        Assert.Contains("downstream API scope", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Authoring.Read", guide, StringComparison.Ordinal);
        Assert.Contains("tenant IDs", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("X-Adopta-Tenant-Id", guide, StringComparison.Ordinal);
        Assert.Contains("X-Adopta-Test-*", guide, StringComparison.Ordinal);
    }

    [Fact]
    public void Readiness_guide_documents_default_fail_closed_and_placeholder_only_behaviour()
    {
        var guide = ReadRepositoryFile(ReadinessGuidePath);

        Assert.Contains("disabled by default", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fails closed", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("placeholders only", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must not contain real values", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LocalStudioContentClient", guide, StringComparison.Ordinal);
        Assert.Contains("no live API call is made by default", guide, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Readiness_guide_documents_non_goals_for_write_workflow_publish_and_infrastructure()
    {
        var guide = ReadRepositoryFile(ReadinessGuidePath);

        Assert.Contains("live create draft", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live update draft", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live request review", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live approve", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live reject", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live publish", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backend API changes", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EF migrations", guide, StringComparison.Ordinal);
        Assert.Contains("database schema changes", guide, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Readiness_docs_do_not_contain_obvious_secret_markers()
    {
        var guide = ReadRepositoryFile(ReadinessGuidePath);
        var sprint = ReadRepositoryFile(Sprint9Path);
        var docs = string.Concat(guide, Environment.NewLine, sprint);

        Assert.DoesNotContain(Forbidden("Password", "="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("User", " Id="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Account", "Key="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Bearer", " "), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Client", "Secret"), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=tcp:", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Initial Catalog=", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Data Source=", docs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_9_closeout_status_exists()
    {
        var sprint = ReadRepositoryFile(Sprint9Path);

        Assert.Contains("Slice 5 - Read-only Studio API configuration readiness and Sprint 9 closeout", sprint, StringComparison.Ordinal);
        Assert.Contains("Sprint 9 closeout checklist", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sprint 9 closeout status", sprint, StringComparison.OrdinalIgnoreCase);
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
