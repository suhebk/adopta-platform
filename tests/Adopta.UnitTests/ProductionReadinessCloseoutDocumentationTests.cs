namespace Adopta.UnitTests;

public sealed class ProductionReadinessCloseoutDocumentationTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Theory]
    [InlineData("docs/adopta/production-readiness/SPRINT-5-PRODUCTION-READINESS-CLOSEOUT.md")]
    [InlineData("docs/adopta/production-readiness/PERSISTENCE-ENABLEMENT-CHECKLIST.md")]
    [InlineData("docs/adopta/production-readiness/API-SECURITY-ROUTE-AUTHORIZATION-CHECKLIST.md")]
    [InlineData("docs/adopta/production-readiness/MIGRATION-EXECUTION-APPROVAL-CHECKLIST.md")]
    public void Production_readiness_documents_exist(string relativePath)
    {
        Assert.True(File.Exists(Path.Combine(RepositoryRoot, relativePath)));
    }

    [Fact]
    public void Closeout_document_covers_required_status_topics()
    {
        var content = Read("docs/adopta/production-readiness/SPRINT-5-PRODUCTION-READINESS-CLOSEOUT.md");

        Assert.Contains("Current Implemented State", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Production-Ready Foundation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Not Production-Enabled Yet", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Schema Baseline Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Migration Execution Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Durable Audit And History Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("API Hardening Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Publishing API Contract Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Persistence Opt-In Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Tenant Isolation Status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Known Limitations", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Next Recommended Sprint Direction", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Persistence_enablement_checklist_covers_required_guardrails()
    {
        var content = Read("docs/adopta/production-readiness/PERSISTENCE-ENABLEMENT-CHECKLIST.md");

        Assert.Contains("disabled by default", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("opt-in", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No automatic migrations", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No automatic database creation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No startup database mutation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("approval", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("incident-response", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant isolation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Secrets must not be stored", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Api_security_checklist_covers_route_authorization_and_publishing_limits()
    {
        var content = Read("docs/adopta/production-readiness/API-SECURITY-ROUTE-AUTHORIZATION-CHECKLIST.md");

        Assert.Contains("route authorization", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant context", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("permission", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cross-tenant", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Authoring.Publish", content, StringComparison.Ordinal);
        Assert.Contains("publishing API remains contract-only", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no delivery API", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Migration_approval_checklist_marks_execution_not_approved()
    {
        var content = Read("docs/adopta/production-readiness/MIGRATION-EXECUTION-APPROVAL-CHECKLIST.md");

        Assert.Contains("Migration execution is not approved", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("automatic startup migration", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("automatic database creation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("startup database mutation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("approval", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("incident-response", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_5_documentation_contains_closeout_status()
    {
        var content = Read("docs/adopta/sprints/ADOPTA-SPRINT-5.md");

        Assert.Contains("Slice 5 - Final production-readiness closeout", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sprint 5 closeout status", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("controlled production-enablement foundation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("migration execution", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not production-enabled", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("docs/adopta/production-readiness/SPRINT-5-PRODUCTION-READINESS-CLOSEOUT.md")]
    [InlineData("docs/adopta/production-readiness/PERSISTENCE-ENABLEMENT-CHECKLIST.md")]
    [InlineData("docs/adopta/production-readiness/API-SECURITY-ROUTE-AUTHORIZATION-CHECKLIST.md")]
    [InlineData("docs/adopta/production-readiness/MIGRATION-EXECUTION-APPROVAL-CHECKLIST.md")]
    public void Production_readiness_docs_do_not_contain_secret_markers(string relativePath)
    {
        var content = Read(relativePath);

        Assert.DoesNotContain("Password=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("User Id=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AccountKey=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer ", content, StringComparison.OrdinalIgnoreCase);
    }

    private static string Read(string relativePath)
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot, relativePath));
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
