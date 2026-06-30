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
        var results = new[]
        {
            StudioContentClientResult<StudioContentPageModel>.Unauthorized(),
            StudioContentClientResult<StudioContentPageModel>.Forbidden(),
            StudioContentClientResult<StudioContentPageModel>.NotFound(),
            StudioContentClientResult<StudioContentPageModel>.InvalidResponse(),
            StudioContentClientResult<StudioContentPageModel>.Unavailable(),
            StudioContentClientResult<StudioContentPageModel>.UnexpectedError()
        };

        Assert.All(results, result =>
        {
            Assert.False(result.Succeeded);
            Assert.Null(result.Value);
            AssertSafeMessage(result.SafeMessage);
        });
    }

    [Fact]
    public void Client_request_models_do_not_accept_tenant_id()
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
}
