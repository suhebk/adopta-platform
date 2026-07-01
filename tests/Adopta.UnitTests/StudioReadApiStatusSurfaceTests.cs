using Adopta.Application.Identity;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioReadApiStatusSurfaceTests
{
    private const string GovernancePagePath =
        "src/Adopta.Web/Components/Pages/Studio/StudioGovernance.razor";

    private const string GovernanceStylesPath =
        "src/Adopta.Web/Components/Pages/Studio/StudioGovernance.razor.css";

    private const string Sprint10Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-10.md";

    private const string ReadinessGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ACTIVATION-READINESS.md";

    private const string EnvironmentValidationGuidePath =
        "docs/adopta/studio/STUDIO-READ-API-ENVIRONMENT-VALIDATION.md";

    [Fact]
    public void Governance_page_hosts_read_api_status_surface()
    {
        var markup = ReadRepositoryFile(GovernancePagePath);

        Assert.Contains("@page \"/studio/governance\"", markup, StringComparison.Ordinal);
        Assert.Contains("<h1 id=\"studio-governance-title\">Governance & Audit</h1>", markup, StringComparison.Ordinal);
        Assert.Contains("Studio read API activation readiness", markup, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"studio-read-api-status-title\"", markup, StringComparison.Ordinal);
        Assert.Contains("role=\"status\" aria-live=\"polite\"", markup, StringComparison.Ordinal);
        Assert.Contains("<table class=\"studio-governance__table\">", markup, StringComparison.Ordinal);
        Assert.Contains("<caption>Studio read API activation readiness checks</caption>", markup, StringComparison.Ordinal);
        Assert.Contains("Check code", markup, StringComparison.Ordinal);
        Assert.Contains("Status", markup, StringComparison.Ordinal);
        Assert.Contains("Message", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Governance_page_uses_preflight_service_as_only_status_source()
    {
        var markup = ReadRepositoryFile(GovernancePagePath);

        Assert.Contains("@inject IStudioReadApiPreflightService StudioReadApiPreflightService", markup, StringComparison.Ordinal);
        Assert.Contains("StudioReadApiPreflightService.RunAsync", markup, StringComparison.Ordinal);
        Assert.Contains("StudioReadApiPreflightResult?", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("@inject IConfiguration", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("IOptions<", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("@inject HttpClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("@inject IStudioContentClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("@inject StudioAuthoringReadApiClient", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("StudioReadApiActivationValidator", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Governance_page_renders_only_safe_preflight_fields()
    {
        var markup = ReadRepositoryFile(GovernancePagePath);

        Assert.Contains("@Preflight.Status", markup, StringComparison.Ordinal);
        Assert.Contains("@check.Code", markup, StringComparison.Ordinal);
        Assert.Contains("@check.Status", markup, StringComparison.Ordinal);
        Assert.Contains("@check.Message", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("BaseAddress", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authority", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ClientId", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Scopes", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AccessToken", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Claims", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TenantId", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Options", markup, StringComparison.OrdinalIgnoreCase);
        AssertAvoidsSecretMarkers(markup);
    }

    [Fact]
    public void Governance_page_is_read_only_non_activating_and_has_safe_fallback()
    {
        var markup = ReadRepositoryFile(GovernancePagePath);

        Assert.Contains("This surface reports readiness only.", markup, StringComparison.Ordinal);
        Assert.Contains("It does not activate live reads", markup, StringComparison.Ordinal);
        Assert.Contains("write, workflow, or publish operations", markup, StringComparison.Ordinal);
        Assert.Contains("Readiness unavailable", markup, StringComparison.Ordinal);
        Assert.Contains("Studio read API activation readiness could not be loaded.", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateDraftAsync", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateDraftAsync", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("RequestReviewAsync", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("ApproveAsync", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("RejectAsync", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("PublishAsync", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Governance_page_avoids_unsafe_output_patterns()
    {
        var markup = ReadRepositoryFile(GovernancePagePath);

        Assert.DoesNotContain("MarkupString", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("innerHTML", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("throw", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StackTrace", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HMRC", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax data", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property data", markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Governance_navigation_uses_existing_audit_read_permission()
    {
        var governance = Assert.Single(
            StudioNavigation.Items,
            item => string.Equals(item.RoutePath, "/studio/governance", StringComparison.Ordinal));

        Assert.Equal(AdoptaPermissionKeys.AuditRead, governance.RequiredPermissionKey);
        Assert.Contains(governance.RequiredPermissionKey, AdoptaPermissionKeys.All);
    }

    [Fact]
    public void Status_surface_does_not_add_new_permission_keys()
    {
        var permissions = ReadRepositoryFile("src/Adopta.Application/Identity/AdoptaPermissionKeys.cs");

        Assert.DoesNotContain("StudioReadApi", permissions, StringComparison.Ordinal);
        Assert.DoesNotContain("Activation", permissions, StringComparison.Ordinal);
        Assert.DoesNotContain("Preflight", permissions, StringComparison.Ordinal);
    }

    [Fact]
    public void Governance_styles_are_scoped_to_status_surface()
    {
        var styles = ReadRepositoryFile(GovernanceStylesPath);

        Assert.Contains(".studio-governance", styles, StringComparison.Ordinal);
        Assert.DoesNotContain("body", styles, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("html", styles, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("!important", styles, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Documentation_mentions_slice_4_status_surface_and_non_goals_safely()
    {
        var sprint = ReadRepositoryFile(Sprint10Path);
        var readiness = ReadRepositoryFile(ReadinessGuidePath);
        var environmentValidation = ReadRepositoryFile(EnvironmentValidationGuidePath);
        var docs = string.Concat(sprint, Environment.NewLine, readiness, Environment.NewLine, environmentValidation);

        Assert.Contains("Slice 4", sprint, StringComparison.Ordinal);
        Assert.Contains("Operator-Facing Read API Activation Status Surface", sprint, StringComparison.Ordinal);
        Assert.Contains("Studio read API activation readiness", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/studio/governance", docs, StringComparison.Ordinal);
        Assert.Contains("does not activate live reads", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("write/workflow/publish", docs, StringComparison.OrdinalIgnoreCase);
        AssertAvoidsSecretMarkers(docs);
    }

    private static void AssertAvoidsSecretMarkers(string content)
    {
        foreach (var marker in SensitiveMarkers())
        {
            Assert.DoesNotContain(marker, content, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string[] SensitiveMarkers() =>
    [
        Forbidden("Password", "="),
        Forbidden("User", " Id="),
        Forbidden("Account", "Key="),
        Forbidden("Bearer", " "),
        Forbidden("Client", "Secret"),
        Forbidden("Connection", "String="),
        "Server=tcp:",
        "Initial Catalog=",
        "Data Source="
    ];

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
