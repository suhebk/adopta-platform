using Adopta.Application.Runtime;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioContentEditorTests
{
    [Fact]
    public void Editor_model_validates_required_fields()
    {
        var editor = new StudioContentEditorModel();

        var validation = editor.Validate();

        Assert.False(validation.Succeeded);
        Assert.Contains(validation.Issues, issue => issue.Path == "applicationId");
        Assert.Contains(validation.Issues, issue => issue.Path == "title");
        Assert.Contains(validation.Issues, issue => issue.Path == "contentKey");
        Assert.Contains(validation.Issues, issue => issue.Path == "contentType");
    }

    [Theory]
    [InlineData("welcome.tooltip")]
    [InlineData("release-checklist")]
    [InlineData("setup.review.walkthrough")]
    public void Editor_model_accepts_safe_content_key_format(string contentKey)
    {
        var editor = ValidEditor();
        editor.ContentKey = contentKey;

        var validation = editor.Validate();

        Assert.True(validation.Succeeded);
    }

    [Theory]
    [InlineData("Welcome Tooltip")]
    [InlineData("welcome..tooltip")]
    [InlineData("welcome-tooltip-")]
    [InlineData("1welcome.tooltip")]
    [InlineData("welcome/tooltip")]
    public void Editor_model_rejects_unsafe_content_key_format(string contentKey)
    {
        var editor = ValidEditor();
        editor.ContentKey = contentKey;

        var validation = editor.Validate();

        Assert.False(validation.Succeeded);
        Assert.Contains(validation.Issues, issue => issue.Code == "invalid_format");
    }

    [Fact]
    public void Editor_model_rejects_sensitive_markers_without_echoing_values()
    {
        const string unsafeTitle = "Bearer token value";
        var editor = ValidEditor();
        editor.Title = unsafeTitle;

        var validation = editor.Validate();

        Assert.False(validation.Succeeded);
        var serializedIssues = string.Join("|", validation.Issues.Select(issue => issue.Message));
        Assert.Contains(validation.Issues, issue => issue.Code == "unsafe_metadata");
        Assert.DoesNotContain(unsafeTitle, serializedIssues, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer", serializedIssues, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", serializedIssues, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Editor_model_rejects_missing_content_type()
    {
        var editor = ValidEditor();
        editor.ContentType = null;

        var validation = editor.Validate();

        Assert.False(validation.Succeeded);
        Assert.Contains(validation.Issues, issue => issue.Path == "contentType");
    }

    [Fact]
    public void Editor_request_models_do_not_accept_tenant_id()
    {
        var requestTypes = new[]
        {
            typeof(StudioContentCreateDraftRequest),
            typeof(StudioContentUpdateDraftRequest)
        };

        Assert.All(requestTypes, requestType =>
        {
            Assert.DoesNotContain(
                requestType.GetProperties(),
                property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static StudioContentEditorModel ValidEditor() =>
        new()
        {
            ContentId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            ApplicationId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            Title = "Welcome tooltip",
            ContentKey = "welcome.tooltip",
            ContentType = RuntimeContentType.Tooltip,
            LifecycleState = StudioContentLifecycleState.Draft,
            Version = "0.1.0"
        };
}
