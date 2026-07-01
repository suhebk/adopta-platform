using Adopta.Application.Runtime;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioAuthoringReadContractGapTests
{
    private const string GapReviewPath =
        "docs/adopta/studio/STUDIO-AUTHORING-READ-CONTRACT-GAP-REVIEW.md";

    private const string Sprint11Path =
        "docs/adopta/sprints/ADOPTA-SPRINT-11.md";

    [Fact]
    public void Live_authoring_read_api_contract_does_not_expose_content_type()
    {
        var contractSource = ReadRepositoryFile("src/Adopta.Api/Authoring/AuthoringApiContracts.cs");

        Assert.DoesNotContain("ContentType", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RuntimeContentType", contractSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Live_authoring_read_api_contract_does_not_expose_history_summary()
    {
        var contractSource = ReadRepositoryFile("src/Adopta.Api/Authoring/AuthoringApiContracts.cs");

        Assert.DoesNotContain("HistorySummary", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LifecycleEventCount", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("PublishingEventCount", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LatestSafeActivity", contractSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Studio_authoring_read_contract_mirror_keeps_content_type_gap_explicit()
    {
        Assert.DoesNotContain(
            typeof(StudioAuthoringContentApiResponse).GetProperties(),
            property => property.Name.Contains("ContentType", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Mapper_marks_unknown_content_type_safely()
    {
        var item = StudioAuthoringReadApiMapper.MapItem(BuildContentResponse());

        Assert.False(item.HasKnownContentType);
        Assert.Equal(RuntimeContentType.Tooltip, item.ContentType);
        Assert.Equal("Limited authoring API metadata loaded.", item.History.LatestSafeActivity);
    }

    [Fact]
    public void Mapper_does_not_expose_tenant_ids_into_studio_read_models()
    {
        var response = BuildContentResponse();
        var item = StudioAuthoringReadApiMapper.MapItem(response);

        Assert.Equal(response.Id, item.Id);
        Assert.DoesNotContain(
            item.GetType().GetProperties(),
            property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(StudioContentPageModel).GetProperties(),
            property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Safe_history_fallback_contains_no_sensitive_markers()
    {
        var item = StudioAuthoringReadApiMapper.MapItem(BuildContentResponse());

        AssertSafeText(item.History.LatestSafeActivity);
    }

    [Fact]
    public void Studio_read_request_models_do_not_accept_tenant_ids()
    {
        var requestTypes = new[]
        {
            typeof(StudioContentListRequest),
            typeof(StudioContentGetByIdRequest)
        };

        Assert.All(requestTypes, requestType =>
        {
            Assert.DoesNotContain(
                requestType.GetProperties(),
                property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void Sprint_11_gap_review_document_exists()
    {
        var repository = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repository.FullName, GapReviewPath)));
    }

    [Fact]
    public void Gap_review_covers_required_contract_gap_topics()
    {
        var review = ReadRepositoryFile(GapReviewPath);

        Assert.Contains("Content type", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Lifecycle state", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Version metadata", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Audit And History Summary", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Application Metadata", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Delivery And Publish Metadata", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HasKnownContentType=false", review, StringComparison.Ordinal);
        Assert.Contains("Authoring.Read", review, StringComparison.Ordinal);
        Assert.Contains("requires separate approval", review, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sprint_11_document_covers_slice_1_status_and_non_goals()
    {
        var sprint = ReadRepositoryFile(Sprint11Path);

        Assert.Contains("Slice 1 - Authoring read contract gap review", sprint, StringComparison.Ordinal);
        Assert.Contains("Content type", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Lifecycle state", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Audit/history summary", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No live write/workflow/publish integration exists", sprint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No operational live read activation is enabled by default", sprint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Gap_review_docs_do_not_contain_obvious_secret_markers()
    {
        var docs = string.Concat(
            ReadRepositoryFile(GapReviewPath),
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

    private static StudioAuthoringContentApiResponse BuildContentResponse()
    {
        return new StudioAuthoringContentApiResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "welcome.tooltip",
            "Welcome tooltip",
            [
                new StudioAuthoringContentVersionApiResponse(
                    Guid.NewGuid(),
                    "1.0.0",
                    StudioAuthoringLifecycleStateApiResponse.Approved,
                    DateTimeOffset.Parse("2026-06-30T10:00:00Z"))
            ]);
    }

    private static void AssertSafeText(string text)
    {
        Assert.DoesNotContain("token", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", text, StringComparison.OrdinalIgnoreCase);
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
