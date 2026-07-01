namespace Adopta.UnitTests;

public sealed class StudioReadApiSprint10CloseoutDocumentationTests
{
    private const string RunbookPath =
        "docs/adopta/studio/STUDIO-READ-API-ACTIVATION-RUNBOOK.md";

    private const string Sprint10Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-10.md";

    private const string ReadmePath =
        "docs/adopta/README.md";

    [Fact]
    public void Studio_read_api_activation_runbook_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, RunbookPath)));
    }

    [Fact]
    public void Runbook_documents_required_closeout_topics()
    {
        var runbook = ReadRepositoryFile(RunbookPath);

        Assert.Contains("internal preflight validation", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("controlled environment validation", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("activation rehearsal", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("operator-facing governance status surface", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LocalStudioContentClient", runbook, StringComparison.Ordinal);
        Assert.Contains("StudioAuthoringReadApiClient", runbook, StringComparison.Ordinal);
        Assert.Contains("read-only", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("live write", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("workflow", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("publish", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fail-closed", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Next Recommended Sprint Direction", runbook, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Runbook_documents_activation_prerequisites_with_placeholders_only()
    {
        var runbook = ReadRepositoryFile(RunbookPath);

        Assert.Contains("StudioApi:Enabled=true", runbook, StringComparison.Ordinal);
        Assert.Contains("StudioApi:BaseAddress", runbook, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:Enabled=true", runbook, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:Authority", runbook, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:ClientId", runbook, StringComparison.Ordinal);
        Assert.Contains("Authentication:StudioWeb:CallbackPath", runbook, StringComparison.Ordinal);
        Assert.Contains("StudioApi:TokenAcquisition:Enabled=true", runbook, StringComparison.Ordinal);
        Assert.Contains("StudioApi:TokenAcquisition:Scopes", runbook, StringComparison.Ordinal);
        Assert.Contains("placeholder", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No committed secrets", runbook, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Runbook_documents_security_boundaries()
    {
        var runbook = ReadRepositoryFile(RunbookPath);

        Assert.Contains("Browser pages and Web request models must not provide tenant IDs", runbook, StringComparison.Ordinal);
        Assert.Contains("X-Adopta-Tenant-Id", runbook, StringComparison.Ordinal);
        Assert.Contains("X-Adopta-Test-*", runbook, StringComparison.Ordinal);
        Assert.Contains("request boundary remains the only Authorization attachment point", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No configured values are displayed", runbook, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_10_documentation_contains_closeout_status()
    {
        var sprint = ReadRepositoryFile(Sprint10Path);

        Assert.Contains("Slice 5 - Sprint 10 closeout documentation and guardrail review", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sprint 10 closeout checklist", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sprint 10 closeout status", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ready to close", sprint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Readme_links_sprint_10_and_read_api_readiness_material()
    {
        var readme = ReadRepositoryFile(ReadmePath);

        Assert.Contains("ADOPTA-SPRINT-10", readme, StringComparison.Ordinal);
        Assert.Contains("Studio Read API Activation Readiness", readme, StringComparison.Ordinal);
        Assert.Contains("Studio Read API Activation Runbook", readme, StringComparison.Ordinal);
        Assert.Contains("Current sprint: `ADOPTA-SPRINT-10", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Closeout_docs_document_disabled_default_fail_closed_and_no_live_write_boundaries()
    {
        var docs = string.Concat(
            ReadRepositoryFile(RunbookPath),
            Environment.NewLine,
            ReadRepositoryFile(Sprint10Path));

        Assert.Contains("disabled by default", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fail-closed", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("placeholders only", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no committed secrets", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Live write, workflow, and publish operations remain unavailable", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no live activation by default", docs, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Closeout_docs_do_not_contain_obvious_secret_markers()
    {
        var docs = string.Concat(
            ReadRepositoryFile(RunbookPath),
            Environment.NewLine,
            ReadRepositoryFile(Sprint10Path),
            Environment.NewLine,
            ReadRepositoryFile(ReadmePath));

        Assert.DoesNotContain(Forbidden("Password", "="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("User", " Id="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Account", "Key="), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Bearer", " "), docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Forbidden("Client", "Secret"), docs, StringComparison.OrdinalIgnoreCase);
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
