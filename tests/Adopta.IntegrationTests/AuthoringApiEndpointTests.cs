using System.Net;
using System.Net.Http.Json;
using Adopta.Api.Authoring;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Authoring;
using Adopta.Application.Identity;
using Adopta.Domain.Authoring;
using Adopta.Domain.Identity;
using Adopta.Infrastructure.Authoring;
using Adopta.Infrastructure.Identity;
using Adopta.Infrastructure.Persistence;
using Adopta.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Adopta.IntegrationTests;

public sealed class AuthoringApiEndpointTests
{
    [Fact]
    public async Task Missing_tenant_context_is_denied()
    {
        using var factory = BuildFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/authoring/content");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Missing_tenant_context_is_denied_on_all_authoring_routes()
    {
        using var factory = BuildFactory();
        using var client = factory.CreateClient();

        foreach (var request in BuildAuthoringRouteRequests())
        {
            using (request)
            {
                var response = await client.SendAsync(request);

                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }
    }

    [Fact]
    public async Task Missing_permission_is_denied()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.DiagnosticsRead);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(HttpMethod.Get, "/authoring/content", seed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_permission_is_denied_for_create_get_and_list()
    {
        var tenantId = Guid.NewGuid();
        var readOnly = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringRead, tenantId);
        var manageOnly = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringManage, tenantId);
        var content = await SeedContentAsync(tenantId, ContentLifecycleState.Draft);

        using var factory = BuildFactoryWithSeeds(readOnly, manageOnly);
        using var client = factory.CreateClient();
        using var createRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/authoring/content",
            readOnly,
            BuildCreateRequest());
        using var getRequest = CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/authoring/content/{content.Id}",
            manageOnly);
        using var listRequest = CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/authoring/content",
            manageOnly);

        var createResponse = await client.SendAsync(createRequest);
        var getResponse = await client.SendAsync(getRequest);
        var listResponse = await client.SendAsync(listRequest);

        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
    }

    [Fact]
    public async Task Create_authored_content_succeeds_with_tenant_and_manage_permission()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringManage);
        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/authoring/content",
            seed,
            BuildCreateRequest());

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<AuthoringCommandResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.Succeeded);
        Assert.NotNull(body.Content);
        Assert.Equal(seed.InternalTenantId, body.Content.TenantId);
        Assert.Single(body.Content.Versions);
    }

    [Fact]
    public async Task Get_and_list_authored_content_are_tenant_scoped()
    {
        var tenantA = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringRead);
        var tenantB = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringRead);
        var tenantAContent = await SeedContentAsync(tenantA.InternalTenantId, ContentLifecycleState.Draft);
        var tenantBContent = await SeedContentAsync(tenantB.InternalTenantId, ContentLifecycleState.Draft);

        using var factory = BuildFactoryWithSeeds(tenantA, tenantB);
        using var client = factory.CreateClient();
        using var getOwn = CreateAuthenticatedRequest(HttpMethod.Get, $"/authoring/content/{tenantAContent.Id}", tenantA);
        using var getOther = CreateAuthenticatedRequest(HttpMethod.Get, $"/authoring/content/{tenantBContent.Id}", tenantA);
        using var listOwn = CreateAuthenticatedRequest(HttpMethod.Get, "/authoring/content", tenantA);

        var ownResponse = await client.SendAsync(getOwn);
        var otherResponse = await client.SendAsync(getOther);
        var listResponse = await client.SendAsync(listOwn);
        var listBody = await listResponse.Content.ReadFromJsonAsync<AuthoredContentListResponse>();

        Assert.Equal(HttpStatusCode.OK, ownResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(listBody);
        Assert.Contains(listBody.Items, item => item.Id == tenantAContent.Id);
        Assert.DoesNotContain(listBody.Items, item => item.Id == tenantBContent.Id);
    }

    [Fact]
    public async Task Request_review_requires_review_permission()
    {
        var allowed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringReview);
        var denied = TestIdentitySeed.Create(
            AdoptaPermissionKeys.AuthoringRead,
            allowed.InternalTenantId);
        var content = await SeedContentAsync(allowed.InternalTenantId, ContentLifecycleState.Draft);
        var version = Assert.Single(content.Versions);

        using var factory = BuildFactoryWithSeeds(allowed, denied);
        using var client = factory.CreateClient();
        using var deniedRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/request-review",
            denied,
            new RequestReviewRequest(null));
        using var allowedRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/request-review",
            allowed,
            new RequestReviewRequest(null));

        var deniedResponse = await client.SendAsync(deniedRequest);
        var allowedResponse = await client.SendAsync(allowedRequest);

        Assert.Equal(HttpStatusCode.Forbidden, deniedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task Successful_request_review_creates_one_lifecycle_history_record()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringReview);
        var content = await SeedContentAsync(seed.InternalTenantId, ContentLifecycleState.Draft);
        var version = Assert.Single(content.Versions);
        var historyRepository = new TestLifecycleHistoryRepository();

        using var factory = BuildFactoryWithSeed(seed, historyRepository);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/request-review",
            seed,
            new RequestReviewRequest(null));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var record = Assert.Single(historyRepository.Records);
        Assert.Equal(seed.InternalTenantId, record.TenantId);
        Assert.Equal(content.Id, record.ContentId);
        Assert.Equal(version.Id, record.VersionId);
        Assert.Equal("RequestReview", record.LifecycleAction);
        Assert.Equal(ContentLifecycleState.Draft, record.FromState);
        Assert.Equal(ContentLifecycleState.InReview, record.ToState);
        Assert.Equal("Succeeded", record.Result);
    }

    [Fact]
    public async Task Approve_requires_approve_permission()
    {
        var allowed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringApprove);
        var denied = TestIdentitySeed.Create(
            AdoptaPermissionKeys.AuthoringReview,
            allowed.InternalTenantId);
        var content = await SeedContentAsync(allowed.InternalTenantId, ContentLifecycleState.InReview);
        var version = Assert.Single(content.Versions);

        using var factory = BuildFactoryWithSeeds(allowed, denied);
        using var client = factory.CreateClient();
        using var deniedRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/approve",
            denied,
            new ApprovalDecisionRequest(null));
        using var allowedRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/approve",
            allowed,
            new ApprovalDecisionRequest(null));

        var deniedResponse = await client.SendAsync(deniedRequest);
        var allowedResponse = await client.SendAsync(allowedRequest);

        Assert.Equal(HttpStatusCode.Forbidden, deniedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task Successful_approve_creates_one_lifecycle_history_record()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringApprove);
        var content = await SeedContentAsync(seed.InternalTenantId, ContentLifecycleState.InReview);
        var version = Assert.Single(content.Versions);
        var historyRepository = new TestLifecycleHistoryRepository();

        using var factory = BuildFactoryWithSeed(seed, historyRepository);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/approve",
            seed,
            new ApprovalDecisionRequest(null));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var record = Assert.Single(historyRepository.Records);
        Assert.Equal(seed.InternalTenantId, record.TenantId);
        Assert.Equal(content.Id, record.ContentId);
        Assert.Equal(version.Id, record.VersionId);
        Assert.Equal("Approve", record.LifecycleAction);
        Assert.Equal(ContentLifecycleState.InReview, record.FromState);
        Assert.Equal(ContentLifecycleState.Approved, record.ToState);
        Assert.Equal("Succeeded", record.Result);
    }

    [Fact]
    public async Task Reject_requires_review_permission()
    {
        var allowed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringReview);
        var denied = TestIdentitySeed.Create(
            AdoptaPermissionKeys.AuthoringApprove,
            allowed.InternalTenantId);
        var content = await SeedContentAsync(allowed.InternalTenantId, ContentLifecycleState.InReview);
        var version = Assert.Single(content.Versions);

        using var factory = BuildFactoryWithSeeds(allowed, denied);
        using var client = factory.CreateClient();
        using var deniedRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/reject",
            denied,
            new ApprovalDecisionRequest(null));
        using var allowedRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/reject",
            allowed,
            new ApprovalDecisionRequest(null));

        var deniedResponse = await client.SendAsync(deniedRequest);
        var allowedResponse = await client.SendAsync(allowedRequest);

        Assert.Equal(HttpStatusCode.Forbidden, deniedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task Successful_reject_creates_one_lifecycle_history_record()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringReview);
        var content = await SeedContentAsync(seed.InternalTenantId, ContentLifecycleState.InReview);
        var version = Assert.Single(content.Versions);
        var historyRepository = new TestLifecycleHistoryRepository();

        using var factory = BuildFactoryWithSeed(seed, historyRepository);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/reject",
            seed,
            new ApprovalDecisionRequest(null));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var record = Assert.Single(historyRepository.Records);
        Assert.Equal(seed.InternalTenantId, record.TenantId);
        Assert.Equal(content.Id, record.ContentId);
        Assert.Equal(version.Id, record.VersionId);
        Assert.Equal("Reject", record.LifecycleAction);
        Assert.Equal(ContentLifecycleState.InReview, record.FromState);
        Assert.Equal(ContentLifecycleState.Draft, record.ToState);
        Assert.Equal("Succeeded", record.Result);
    }

    [Fact]
    public async Task Cross_tenant_access_is_hidden_safely()
    {
        var tenantA = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringRead);
        var tenantB = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringRead);
        var tenantBContent = await SeedContentAsync(tenantB.InternalTenantId, ContentLifecycleState.Draft);

        using var factory = BuildFactoryWithSeeds(tenantA, tenantB);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(HttpMethod.Get, $"/authoring/content/{tenantBContent.Id}", tenantA);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(tenantB.InternalTenantId.ToString(), body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_workflow_command_fails_safely()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringApprove);
        var content = await SeedContentAsync(seed.InternalTenantId, ContentLifecycleState.Draft);
        var version = Assert.Single(content.Versions);

        using var factory = BuildFactoryWithSeed(seed);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/approve",
            seed,
            new ApprovalDecisionRequest(null));

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<AuthoringCommandResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Succeeded);
        Assert.Contains(body.Issues, issue => issue.Code == "invalid_lifecycle_decision");
        Assert.DoesNotContain(seed.InternalTenantId.ToString(), string.Join(" ", body.Issues.Select(issue => issue.Message)), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_workflow_command_does_not_create_lifecycle_history()
    {
        var seed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringApprove);
        var content = await SeedContentAsync(seed.InternalTenantId, ContentLifecycleState.Draft);
        var version = Assert.Single(content.Versions);
        var historyRepository = new TestLifecycleHistoryRepository();

        using var factory = BuildFactoryWithSeed(seed, historyRepository);
        using var client = factory.CreateClient();
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/approve",
            seed,
            new ApprovalDecisionRequest(null));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(historyRepository.Records);
    }

    private static WebApplicationFactory<Program> BuildFactoryWithSeed(
        TestIdentitySeed seed,
        IAuthoredContentLifecycleHistoryRepository? lifecycleHistoryRepository = null)
    {
        return BuildFactoryWithSeeds([seed], lifecycleHistoryRepository);
    }

    private static WebApplicationFactory<Program> BuildFactoryWithSeeds(params TestIdentitySeed[] seeds)
    {
        return BuildFactoryWithSeeds(seeds, null);
    }

    private static WebApplicationFactory<Program> BuildFactoryWithSeeds(
        TestIdentitySeed[] seeds,
        IAuthoredContentLifecycleHistoryRepository? lifecycleHistoryRepository)
    {
        var tenantStore = new InMemoryAdoptaTenantMappingStore();
        var userStore = new InMemoryAuthenticatedUserMappingStore();

        foreach (var seed in seeds)
        {
            tenantStore.Add(seed.ExternalTenantId, seed.ApplicationId, seed.InternalTenantId);
            userStore.Add(
                seed.InternalTenantId,
                seed.SubjectId,
                BuildUser(seed.InternalTenantId, seed.SubjectId, seed.PermissionKey));
        }

        return BuildFactory(tenantStore, userStore, lifecycleHistoryRepository);
    }

    private static WebApplicationFactory<Program> BuildFactory(
        InMemoryAdoptaTenantMappingStore? tenantStore = null,
        InMemoryAuthenticatedUserMappingStore? userStore = null,
        IAuthoredContentLifecycleHistoryRepository? lifecycleHistoryRepository = null)
    {
        tenantStore ??= new InMemoryAdoptaTenantMappingStore();
        userStore ??= new InMemoryAuthenticatedUserMappingStore();

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Authentication:Test:Enabled", "true");

                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Authentication:Test:Enabled"] = "true"
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(tenantStore);
                    services.AddSingleton(userStore);
                    if (lifecycleHistoryRepository is not null)
                    {
                        services.RemoveAll<IAuthoredContentLifecycleHistoryRepository>();
                        services.AddSingleton(lifecycleHistoryRepository);
                    }
                });
            });
    }

    private static IReadOnlyCollection<HttpRequestMessage> BuildAuthoringRouteRequests()
    {
        var contentId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        return
        [
            new HttpRequestMessage(HttpMethod.Post, "/authoring/content")
            {
                Content = JsonContent.Create(BuildCreateRequest())
            },
            new HttpRequestMessage(HttpMethod.Get, $"/authoring/content/{contentId}"),
            new HttpRequestMessage(HttpMethod.Get, "/authoring/content"),
            new HttpRequestMessage(HttpMethod.Post, $"/authoring/content/{contentId}/versions/{versionId}/request-review")
            {
                Content = JsonContent.Create(new RequestReviewRequest(null))
            },
            new HttpRequestMessage(HttpMethod.Post, $"/authoring/content/{contentId}/versions/{versionId}/approve")
            {
                Content = JsonContent.Create(new ApprovalDecisionRequest(null))
            },
            new HttpRequestMessage(HttpMethod.Post, $"/authoring/content/{contentId}/versions/{versionId}/reject")
            {
                Content = JsonContent.Create(new ApprovalDecisionRequest(null))
            }
        ];
    }

    private static HttpRequestMessage CreateAuthenticatedRequest(
        HttpMethod method,
        string requestUri,
        TestIdentitySeed seed,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Add("X-Adopta-Test-Authenticated", "true");
        request.Headers.Add("X-Adopta-Test-Tid", seed.ExternalTenantId);
        request.Headers.Add("X-Adopta-Test-AppId", seed.ApplicationId);
        request.Headers.Add("X-Adopta-Test-Oid", seed.SubjectId);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static CreateAuthoredContentRequest BuildCreateRequest()
    {
        return new CreateAuthoredContentRequest(
            Guid.NewGuid(),
            "billing.submit",
            "Submit return",
            "1.0.0");
    }

    private static async Task<AuthoredContentItem> SeedContentAsync(
        Guid tenantId,
        ContentLifecycleState state)
    {
        var context = new AdoptionTenantContext();
        context.SetTenant(tenantId);
        IAuthoredContentRepository repository = new InMemoryAuthoredContentRepository(context);
        var content = new AuthoredContentItem(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            "billing.submit",
            "Submit return",
            [
                new AuthoredContentVersion(
                    Guid.NewGuid(),
                    "1.0.0",
                    state,
                    DateTimeOffset.UtcNow)
            ]);

        await repository.AddAsync(content);

        return content;
    }

    private static AdoptionUser BuildUser(Guid tenantId, string subjectId, string permissionKey)
    {
        var user = new AdoptionUser(Guid.NewGuid(), tenantId, subjectId, "Test User");
        var role = new Role(Guid.NewGuid(), tenantId, "Test Role");
        role.Grant(new Permission(permissionKey, "Test permission"));
        user.AssignRole(role);

        return user;
    }

    private sealed record TestIdentitySeed(
        string ExternalTenantId,
        string ApplicationId,
        string SubjectId,
        Guid InternalTenantId,
        string PermissionKey)
    {
        public static TestIdentitySeed Create(string permissionKey)
        {
            return new TestIdentitySeed(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid(),
                permissionKey);
        }

        public static TestIdentitySeed Create(string permissionKey, Guid internalTenantId)
        {
            return new TestIdentitySeed(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                internalTenantId,
                permissionKey);
        }
    }

    private sealed class TestLifecycleHistoryRepository : IAuthoredContentLifecycleHistoryRepository
    {
        private readonly List<AuthoredContentLifecycleAuditRecord> _records = [];

        public IReadOnlyCollection<AuthoredContentLifecycleAuditRecord> Records => _records;

        public Task AddAsync(
            AuthoredContentLifecycleAuditRecord auditRecord,
            CancellationToken cancellationToken = default)
        {
            _records.Add(auditRecord);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<AuthoredContentLifecycleAuditRecord>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<AuthoredContentLifecycleAuditRecord>>(_records.ToArray());
        }
    }
}
