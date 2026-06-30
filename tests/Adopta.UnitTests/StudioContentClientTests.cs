using Adopta.Application.Runtime;
using Adopta.Web.Studio;

namespace Adopta.UnitTests;

public sealed class StudioContentClientTests
{
    [Fact]
    public async Task Local_client_lists_foundation_content_successfully()
    {
        var client = new LocalStudioContentClient();

        var result = await client.ListAsync(new StudioContentListRequest(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(StudioContentPageState.Loaded, result.Value.State);
        Assert.NotEmpty(result.Value.Items);
    }

    [Fact]
    public async Task Local_client_filters_by_application_without_tenant_input()
    {
        var client = new LocalStudioContentClient();
        var applicationId = StudioContentFoundationData.Loaded().Items.First().ApplicationId;

        var result = await client.ListAsync(new StudioContentListRequest(applicationId), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items, item => Assert.Equal(applicationId, item.ApplicationId));
    }

    [Fact]
    public async Task Local_client_get_by_id_returns_success_for_existing_content()
    {
        var client = new LocalStudioContentClient();
        var content = StudioContentFoundationData.Loaded().Items.First();

        var result = await client.GetByIdAsync(new StudioContentGetByIdRequest(content.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.Equal(content.Id, result.Value?.Id);
    }

    [Fact]
    public async Task Local_client_get_by_id_returns_safe_not_found()
    {
        var client = new LocalStudioContentClient();

        var result = await client.GetByIdAsync(new StudioContentGetByIdRequest(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        Assert.Equal("Studio content was not found.", result.SafeMessage);
    }

    [Fact]
    public async Task Local_client_create_draft_returns_safe_typed_result()
    {
        var client = new LocalStudioContentClient();
        var applicationId = StudioContentFoundationData.Loaded().Items.First().ApplicationId;

        var result = await client.CreateDraftAsync(
            new StudioContentCreateDraftRequest(
                applicationId,
                "New tooltip",
                "new.tooltip",
                RuntimeContentType.Tooltip,
                "0.1.0"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.ContentId);
        Assert.Equal(StudioContentEditorState.Saved, result.Value.State);
        Assert.Equal(StudioContentLifecycleState.Draft, result.Value.LifecycleState);
    }

    [Fact]
    public async Task Local_client_update_draft_returns_safe_typed_result()
    {
        var client = new LocalStudioContentClient();
        var draft = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Draft);

        var result = await client.UpdateDraftAsync(
            new StudioContentUpdateDraftRequest(
                draft.Id,
                draft.ApplicationId,
                "Updated tooltip",
                "updated.tooltip",
                RuntimeContentType.Tooltip,
                draft.CurrentVersion?.Version ?? "0.1.0"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(draft.Id, result.Value.ContentId);
        Assert.Equal("Updated tooltip", result.Value.Title);
        Assert.Equal(StudioContentEditorState.Saved, result.Value.State);
    }

    [Fact]
    public async Task Local_client_create_draft_rejects_validation_errors_safely()
    {
        var client = new LocalStudioContentClient();

        var result = await client.CreateDraftAsync(
            new StudioContentCreateDraftRequest(
                Guid.Empty,
                "Bearer token value",
                "invalid key",
                null,
                "0.1.0"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.ValidationError, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(StudioContentEditorState.ValidationError, result.Value.State);
        Assert.DoesNotContain("Bearer", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Local_client_request_review_succeeds_for_draft()
    {
        var client = new LocalStudioContentClient();
        var draft = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Draft);

        var result = await client.RequestReviewAsync(
            new StudioWorkflowActionRequest(draft.Id, draft.CurrentVersion?.Id ?? Guid.Empty),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(StudioWorkflowActionState.Succeeded, result.Value.State);
        Assert.Equal(StudioContentLifecycleState.InReview, result.Value.LifecycleState);
        Assert.Equal(StudioWorkflowActionKind.RequestReview, result.Value.LastAction);
    }

    [Fact]
    public async Task Local_client_approve_succeeds_for_in_review()
    {
        var client = new LocalStudioContentClient();
        var inReview = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.InReview);

        var result = await client.ApproveAsync(
            new StudioWorkflowActionRequest(inReview.Id, inReview.CurrentVersion?.Id ?? Guid.Empty),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(StudioWorkflowActionState.Succeeded, result.Value.State);
        Assert.Equal(StudioContentLifecycleState.Approved, result.Value.LifecycleState);
        Assert.Equal(StudioWorkflowActionKind.Approve, result.Value.LastAction);
    }

    [Fact]
    public async Task Local_client_reject_succeeds_for_in_review()
    {
        var client = new LocalStudioContentClient();
        var inReview = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.InReview);

        var result = await client.RejectAsync(
            new StudioWorkflowActionRequest(inReview.Id, inReview.CurrentVersion?.Id ?? Guid.Empty),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(StudioWorkflowActionState.Succeeded, result.Value.State);
        Assert.Equal(StudioContentLifecycleState.Draft, result.Value.LifecycleState);
        Assert.Equal(StudioWorkflowActionKind.Reject, result.Value.LastAction);
    }

    [Fact]
    public async Task Local_client_invalid_workflow_transition_returns_safe_validation_error()
    {
        var client = new LocalStudioContentClient();
        var approved = StudioContentFoundationData.Loaded()
            .Items
            .First(item => item.LifecycleState == StudioContentLifecycleState.Approved);

        var result = await client.ApproveAsync(
            new StudioWorkflowActionRequest(approved.Id, approved.CurrentVersion?.Id ?? Guid.Empty),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.ValidationError, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(StudioWorkflowActionState.ValidationError, result.Value.State);
        Assert.Contains(result.Value.Issues, issue => issue.Code == "invalid_lifecycle_transition");
        AssertSafeMessage(result.SafeMessage);
    }

    [Fact]
    public async Task Local_client_rejects_empty_content_id_as_invalid_response()
    {
        var client = new LocalStudioContentClient();

        var result = await client.GetByIdAsync(new StudioContentGetByIdRequest(Guid.Empty), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(StudioContentClientStatus.InvalidResponse, result.Status);
        Assert.Null(result.Value);
        Assert.Equal("Studio content could not be loaded.", result.SafeMessage);
    }

    [Fact]
    public void Client_failure_messages_are_generic_and_safe()
    {
        var results = new (StudioContentClientStatus Status, bool Succeeded, object? Value, string SafeMessage)[]
        {
            ToTuple(StudioContentClientResult<StudioContentPageModel>.Unauthorized()),
            ToTuple(StudioContentClientResult<StudioContentPageModel>.Forbidden()),
            ToTuple(StudioContentClientResult<StudioContentPageModel>.NotFound()),
            ToTuple(StudioContentClientResult<StudioContentPageModel>.InvalidResponse()),
            ToTuple(StudioContentClientResult<StudioContentPageModel>.Unavailable()),
            ToTuple(StudioContentClientResult<StudioContentPageModel>.UnexpectedError()),
            ToTuple(StudioContentClientResult<StudioContentEditorModel>.ValidationError(new StudioContentEditorModel())),
            ToTuple(StudioContentClientResult<StudioWorkflowActionModel>.ValidationError(new StudioWorkflowActionModel()))
        };

        Assert.All(results, result =>
        {
            Assert.False(result.Succeeded);
            if (result.Status != StudioContentClientStatus.ValidationError)
            {
                Assert.Null(result.Value);
            }

            AssertSafeMessage(result.SafeMessage);
        });
    }

    [Fact]
    public void Client_request_models_do_not_accept_tenant_id()
    {
        var requestTypes = new[]
        {
            typeof(StudioContentListRequest),
            typeof(StudioContentGetByIdRequest),
            typeof(StudioContentCreateDraftRequest),
            typeof(StudioContentUpdateDraftRequest),
            typeof(StudioWorkflowActionRequest)
        };

        Assert.All(requestTypes, requestType =>
        {
            Assert.DoesNotContain(
                requestType.GetProperties(),
                property => property.Name.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static void AssertSafeMessage(string message)
    {
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.DoesNotContain("token", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("header", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hmrc", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", message, StringComparison.OrdinalIgnoreCase);
    }

    private static (StudioContentClientStatus Status, bool Succeeded, object? Value, string SafeMessage) ToTuple<T>(
        StudioContentClientResult<T> result) =>
        (result.Status, result.Succeeded, result.Value, result.SafeMessage);
}
