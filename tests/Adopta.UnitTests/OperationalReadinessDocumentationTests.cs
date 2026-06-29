namespace Adopta.UnitTests;

public sealed class OperationalReadinessDocumentationTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Theory]
    [InlineData("docs/adopta/operations/OPERATIONAL-READINESS.md")]
    [InlineData("docs/adopta/operations/OBSERVABILITY-AND-LOGGING.md")]
    [InlineData("docs/adopta/operations/INCIDENT-RESPONSE-AND-ROLLBACK.md")]
    [InlineData("docs/adopta/operations/TENANT-ISOLATION-VALIDATION-CHECKLIST.md")]
    public void Operational_readiness_docs_exist(string relativePath)
    {
        Assert.True(File.Exists(Path.Combine(RepositoryRoot, relativePath)));
    }

    [Fact]
    public void Operational_readiness_doc_covers_deployment_persistence_audit_and_approval_gates()
    {
        var content = Read("docs/adopta/operations/OPERATIONAL-READINESS.md");

        Assert.Contains("deployment", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("audit", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("approval", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("disabled by default", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("opt-in", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Observability_doc_covers_safe_logging_and_secret_handling()
    {
        var content = Read("docs/adopta/operations/OBSERVABILITY-AND-LOGGING.md");

        Assert.Contains("logging", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("observability", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("connection string", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tokens", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant secrets", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("connectivity not checked", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Incident_response_doc_covers_rollback_and_future_production_boundaries()
    {
        var content = Read("docs/adopta/operations/INCIDENT-RESPONSE-AND-ROLLBACK.md");

        Assert.Contains("incident", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rollback", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("explicit approval", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No automatic startup migration", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No automatic database creation", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tenant_isolation_doc_covers_cross_tenant_and_persistence_boundary_validation()
    {
        var content = Read("docs/adopta/operations/TENANT-ISOLATION-VALIDATION-CHECKLIST.md");

        Assert.Contains("tenant isolation", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cross-tenant", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("missing tenant context", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fail closed", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_4_documentation_contains_closeout_checklist()
    {
        var content = Read("docs/adopta/sprints/ADOPTA-SPRINT-4.md");

        Assert.Contains("Sprint 4 closeout checklist", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Persistence disabled by default", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SQL Server persistence remains opt-in", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No production Azure SQL deployment", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No live database health checks", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("docs/adopta/operations/OPERATIONAL-READINESS.md")]
    [InlineData("docs/adopta/operations/OBSERVABILITY-AND-LOGGING.md")]
    [InlineData("docs/adopta/operations/INCIDENT-RESPONSE-AND-ROLLBACK.md")]
    [InlineData("docs/adopta/operations/TENANT-ISOLATION-VALIDATION-CHECKLIST.md")]
    public void Operational_docs_do_not_contain_real_secret_markers(string relativePath)
    {
        var content = Read(relativePath);

        Assert.DoesNotContain("Password=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("User Id=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=", content, StringComparison.OrdinalIgnoreCase);
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
