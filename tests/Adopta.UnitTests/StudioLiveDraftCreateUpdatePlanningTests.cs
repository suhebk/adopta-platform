namespace Adopta.UnitTests;

public sealed class StudioLiveDraftCreateUpdatePlanningTests
{
    private const string PlanPath =
        "docs/adopta/studio/STUDIO-LIVE-DRAFT-CREATE-UPDATE-INTEGRATION-PLAN.md";

    private const string Sprint12Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-12.md";

    private const string ReadmePath =
        "docs/adopta/README.md";

    [Fact]
    public void Live_draft_create_update_planning_guide_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, PlanPath)));
    }

    [Fact]
    public void Create_endpoint_exists_and_requires_manage_permission()
    {
        var endpoints = ReadRepositoryFile("src/Adopta.Api/Authoring/AuthoringEndpoints.cs");

        Assert.Contains("MapPost(\"/authoring/content\", CreateAsync)", endpoints, StringComparison.Ordinal);
        Assert.Contains("RequireAdoptaTenantContext()", endpoints, StringComparison.Ordinal);
        Assert.Contains("RequireAdoptaPermission(AdoptaPermissionKeys.AuthoringManage)", endpoints, StringComparison.Ordinal);
    }

    [Fact]
    public void Update_endpoint_does_not_exist()
    {
        var endpoints = ReadRepositoryFile("src/Adopta.Api/Authoring/AuthoringEndpoints.cs");
        var contracts = ReadRepositoryFile("src/Adopta.Api/Authoring/AuthoringApiContracts.cs");

        Assert.DoesNotContain("MapPut", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPatch", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateAuthoredContent", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateAuthoredContent", contracts, StringComparison.Ordinal);
    }

    [Fact]
    public void Docs_block_live_create_update_and_recommend_separate_future_slices()
    {
        var docs = ReadPlanningDocs();

        Assert.Contains("Live create and live update should be separate future implementation slices", docs, StringComparison.Ordinal);
        Assert.Contains("Live draft create remains blocked until content type migration/backfill readiness is approved", docs, StringComparison.Ordinal);
        Assert.Contains("Live draft update is blocked by the missing backend update endpoint", docs, StringComparison.Ordinal);
        Assert.Contains("Future write integration should use a separately gated draft-write boundary", docs, StringComparison.Ordinal);
        Assert.Contains("StudioAuthoringDraftApiClient", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_api_client_remains_read_only()
    {
        var client = ReadRepositoryFile("src/Adopta.Web/Studio/StudioAuthoringReadApiClient.cs");

        Assert.Contains("GetAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PostAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PutAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("PatchAsync", client, StringComparison.Ordinal);
        Assert.DoesNotContain("DeleteAsync", client, StringComparison.Ordinal);
        Assert.Contains("CreateDraftAsync", client, StringComparison.Ordinal);
        Assert.Contains("Unavailable()", client, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_request_models_do_not_accept_tenant_ids()
    {
        var requestTypes = new[]
        {
            typeof(Adopta.Web.Studio.StudioContentListRequest),
            typeof(Adopta.Web.Studio.StudioContentGetByIdRequest),
            typeof(Adopta.Web.Studio.StudioContentCreateDraftRequest),
            typeof(Adopta.Web.Studio.StudioContentUpdateDraftRequest),
            typeof(Adopta.Web.Studio.StudioWorkflowActionRequest),
            typeof(Adopta.Web.Studio.StudioPublishActionRequest)
        };

        Assert.All(requestTypes, requestType =>
        {
            Assert.DoesNotContain(
                requestType.GetProperties(),
                property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void Request_boundary_strips_tenant_and_test_headers()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://placeholder.invalid/authoring/content");
        request.Headers.Add(Adopta.Web.Studio.StudioApiRequestBoundaryHandler.TenantHeaderName, Guid.NewGuid().ToString());
        request.Headers.Add("X-Adopta-Test-Tid", Guid.NewGuid().ToString());

        var removed = Adopta.Web.Studio.StudioApiRequestBoundaryHandler.StripProhibitedHeaders(request);

        Assert.Equal(2, removed);
        Assert.False(request.Headers.Contains(Adopta.Web.Studio.StudioApiRequestBoundaryHandler.TenantHeaderName));
        Assert.DoesNotContain(
            request.Headers,
            header => header.Key.StartsWith("X-Adopta-Test-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Docs_require_content_type_migration_backfill_readiness()
    {
        var docs = ReadPlanningDocs();

        Assert.Contains("content type migration/backfill readiness", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("migration execution must have separate operational approval", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backfill plan must be approved", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Content type must not be inferred", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Docs_keep_workflow_publish_and_property_mtd_out_of_scope()
    {
        var docs = ReadPlanningDocs();

        Assert.Contains("live review/approve/reject/publish integration", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Property MTD integration", docs, StringComparison.Ordinal);
        Assert.Contains("No new permission keys", docs, StringComparison.Ordinal);
        Assert.Contains("Authoring.Manage", docs, StringComparison.Ordinal);
        Assert.Contains("Authoring.Read", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Readme_references_live_draft_create_update_plan()
    {
        var readme = ReadRepositoryFile(ReadmePath);

        Assert.Contains("Studio Live Draft Create/Update Integration Plan", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Planning_docs_do_not_contain_obvious_secret_markers()
    {
        var docs = string.Concat(
            ReadPlanningDocs(),
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

    private static string ReadPlanningDocs() =>
        string.Concat(
            ReadRepositoryFile(PlanPath),
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
