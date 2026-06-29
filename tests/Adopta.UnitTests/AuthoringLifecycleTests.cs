using Adopta.Application.Authoring;
using Adopta.Domain.Authoring;

namespace Adopta.UnitTests;

public sealed class AuthoringLifecycleTests
{
    public static TheoryData<ContentLifecycleState, ContentLifecycleState> ValidTransitions =>
        new()
        {
            { ContentLifecycleState.Draft, ContentLifecycleState.InReview },
            { ContentLifecycleState.InReview, ContentLifecycleState.Draft },
            { ContentLifecycleState.InReview, ContentLifecycleState.Approved },
            { ContentLifecycleState.Approved, ContentLifecycleState.Published },
            { ContentLifecycleState.Approved, ContentLifecycleState.Archived },
            { ContentLifecycleState.Published, ContentLifecycleState.Archived },
            { ContentLifecycleState.Draft, ContentLifecycleState.Archived }
        };

    public static TheoryData<ContentLifecycleState, ContentLifecycleState> InvalidTransitions =>
        new()
        {
            { ContentLifecycleState.Draft, ContentLifecycleState.Published },
            { ContentLifecycleState.Published, ContentLifecycleState.Draft },
            { ContentLifecycleState.Archived, ContentLifecycleState.Draft },
            { ContentLifecycleState.Archived, ContentLifecycleState.InReview },
            { ContentLifecycleState.Archived, ContentLifecycleState.Approved },
            { ContentLifecycleState.Archived, ContentLifecycleState.Published }
        };

    [Theory]
    [MemberData(nameof(ValidTransitions))]
    public void Allowed_lifecycle_transitions_are_valid(ContentLifecycleState from, ContentLifecycleState to)
    {
        var result = AuthoredContentValidator.ValidateTransition(from, to);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
        Assert.True(ContentLifecycleTransition.IsAllowed(from, to));
    }

    [Theory]
    [MemberData(nameof(InvalidTransitions))]
    public void Unsafe_lifecycle_transitions_are_denied_safely(ContentLifecycleState from, ContentLifecycleState to)
    {
        var result = AuthoredContentValidator.ValidateTransition(from, to);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_lifecycle_transition");
        Assert.False(ContentLifecycleTransition.IsAllowed(from, to));
    }

    [Fact]
    public void Version_metadata_rules_require_id_version_state_and_created_timestamp()
    {
        var result = AuthoredContentValidator.ValidateVersion(new AuthoredContentVersionContract(
            Guid.Empty,
            "",
            (ContentLifecycleState)99,
            default));

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_content_version" && issue.Path == "version.id");
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_content_version" && issue.Path == "version.version");
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_lifecycle_state");
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_content_version" && issue.Path == "version.createdAtUtc");
    }

    [Fact]
    public void Valid_authored_content_has_no_validation_issues()
    {
        var result = AuthoredContentValidator.ValidateContent(BuildContent());

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validation_failure_shape_is_typed_and_safe()
    {
        var result = AuthoredContentValidator.ValidateContent(BuildContent(
            id: Guid.Empty,
            tenantId: Guid.Empty,
            applicationId: Guid.Empty,
            contentKey: "",
            title: ""));

        Assert.False(result.IsValid);
        Assert.All(result.Issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.Code));
            Assert.False(string.IsNullOrWhiteSpace(issue.Path));
            Assert.False(string.IsNullOrWhiteSpace(issue.Message));
        });
        Assert.DoesNotContain(result.Issues, issue => issue.Message.Contains("tenant-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Duplicate_version_metadata_is_rejected()
    {
        var version = BuildVersion(version: "1.0.0");
        var result = AuthoredContentValidator.ValidateContent(BuildContent(versions:
        [
            version,
            BuildVersion(version: "1.0.0")
        ]));

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "duplicate_content_version");
    }

    private static AuthoredContentContract BuildContent(
        Guid? id = null,
        Guid? tenantId = null,
        Guid? applicationId = null,
        string contentKey = "billing.submit",
        string title = "Submit return",
        IReadOnlyCollection<AuthoredContentVersionContract>? versions = null)
    {
        return new AuthoredContentContract(
            id ?? Guid.NewGuid(),
            tenantId ?? Guid.NewGuid(),
            applicationId ?? Guid.NewGuid(),
            contentKey,
            title,
            versions ?? [BuildVersion()]);
    }

    private static AuthoredContentVersionContract BuildVersion(
        string version = "1.0.0",
        ContentLifecycleState state = ContentLifecycleState.Draft)
    {
        return new AuthoredContentVersionContract(
            Guid.NewGuid(),
            version,
            state,
            DateTimeOffset.UtcNow);
    }
}
