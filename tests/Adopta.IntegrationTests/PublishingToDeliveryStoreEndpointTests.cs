using System.Net;
using System.Net.Http.Json;
using Adopta.Api.Authoring;
using Adopta.Api.Runtime;
using Adopta.Application.Abstractions.Authoring;
using Adopta.Application.Abstractions.Persistence;
using Adopta.Application.Authoring;
using Adopta.Application.Identity;
using Adopta.Application.Runtime;
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

public sealed class PublishingToDeliveryStoreEndpointTests
{
    [Fact]
    public async Task Successful_publish_stores_delivery_bundle_and_delivery_api_retrieves_it()
    {
        var tenantId = Guid.NewGuid();
        var publishSeed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringPublish, tenantId);
        var deliverySeed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead, tenantId);
        var content = await SeedContentAsync(tenantId, ContentLifecycleState.Approved);
        var version = Assert.Single(content.Versions);
        var publishingHistoryRepository = new TestPublishingHistoryRepository();

        using var factory = BuildFactoryWithSeeds(
            [publishSeed, deliverySeed],
            publishingHistoryRepository);
        using var client = factory.CreateClient();
        using var publishRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/publish",
            publishSeed,
            BuildPublishRequest());

        var publishResponse = await client.SendAsync(publishRequest);
        var publishBody = await publishResponse.Content.ReadFromJsonAsync<PublishAuthoredContentResponse>();

        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        Assert.NotNull(publishBody);
        Assert.True(publishBody.Succeeded);
        Assert.NotNull(publishBody.Bundle);
        Assert.Single(publishingHistoryRepository.Records);

        using var deliveryRequest = CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/runtime/delivery/bundles/{content.ApplicationId}?environment=production&channel=Published",
            deliverySeed);

        var deliveryResponse = await client.SendAsync(deliveryRequest);
        var deliveryBody = await deliveryResponse.Content.ReadFromJsonAsync<RuntimeDeliveryBundleResponse>();
        var rawDeliveryBody = await deliveryResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, deliveryResponse.StatusCode);
        Assert.NotNull(deliveryBody);
        Assert.True(deliveryBody.Succeeded);
        Assert.Equal("found", deliveryBody.Status);
        Assert.NotNull(deliveryBody.Bundle);
        Assert.Equal(publishBody.Bundle.BundleId, deliveryBody.Bundle.Content.BundleId);
        Assert.Equal(tenantId, deliveryBody.Bundle.TenantId);
        Assert.Equal(content.ApplicationId, deliveryBody.Bundle.ApplicationId);
        Assert.Equal("production", deliveryBody.Bundle.Environment);
        Assert.Equal(DeliveryChannel.Published, deliveryBody.Bundle.Channel);
        var item = Assert.Single(deliveryBody.Bundle.Content.Items);
        Assert.Equal(content.ContentKey, item.Id);
        Assert.Null(item.Body);
        AssertNoSensitiveMarkers(rawDeliveryBody);
    }

    [Fact]
    public async Task Failed_publish_stores_no_delivery_bundle_or_publishing_history()
    {
        var tenantId = Guid.NewGuid();
        var publishSeed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringPublish, tenantId);
        var deliverySeed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead, tenantId);
        var content = await SeedContentAsync(tenantId, ContentLifecycleState.Draft);
        var version = Assert.Single(content.Versions);
        var publishingHistoryRepository = new TestPublishingHistoryRepository();

        using var factory = BuildFactoryWithSeeds(
            [publishSeed, deliverySeed],
            publishingHistoryRepository);
        using var client = factory.CreateClient();
        using var publishRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/publish",
            publishSeed,
            BuildPublishRequest());

        var publishResponse = await client.SendAsync(publishRequest);
        var deliveryResponse = await SendDeliveryLookupAsync(client, deliverySeed, content.ApplicationId, "production", "Published");

        Assert.Equal(HttpStatusCode.BadRequest, publishResponse.StatusCode);
        Assert.Empty(publishingHistoryRepository.Records);
        Assert.Equal(HttpStatusCode.NotFound, deliveryResponse.StatusCode);
    }

    [Fact]
    public async Task Invalid_publish_stores_no_delivery_bundle_or_publishing_history()
    {
        var tenantId = Guid.NewGuid();
        var publishSeed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringPublish, tenantId);
        var deliverySeed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead, tenantId);
        var content = await SeedContentAsync(tenantId, ContentLifecycleState.Approved);
        var version = Assert.Single(content.Versions);
        var publishingHistoryRepository = new TestPublishingHistoryRepository();

        using var factory = BuildFactoryWithSeeds(
            [publishSeed, deliverySeed],
            publishingHistoryRepository);
        using var client = factory.CreateClient();
        using var publishRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/publish",
            publishSeed,
            new PublishAuthoredContentRequest("", DeliveryChannel.Published, null));

        var publishResponse = await client.SendAsync(publishRequest);
        var publishBody = await publishResponse.Content.ReadFromJsonAsync<PublishAuthoredContentResponse>();
        var deliveryResponse = await SendDeliveryLookupAsync(client, deliverySeed, content.ApplicationId, "production", "Published");

        Assert.Equal(HttpStatusCode.BadRequest, publishResponse.StatusCode);
        Assert.NotNull(publishBody);
        Assert.False(publishBody.Succeeded);
        Assert.Equal("invalid_publish_command", publishBody.Status);
        Assert.Null(publishBody.Bundle);
        Assert.Null(publishBody.Audit);
        Assert.Empty(publishingHistoryRepository.Records);
        Assert.Equal(HttpStatusCode.NotFound, deliveryResponse.StatusCode);
    }

    [Fact]
    public async Task Cross_tenant_publish_stores_no_delivery_bundle_or_publishing_history()
    {
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var publishSeed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringPublish, tenantAId);
        var deliverySeed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead, tenantAId);
        var tenantBContent = await SeedContentAsync(tenantBId, ContentLifecycleState.Approved);
        var version = Assert.Single(tenantBContent.Versions);
        var publishingHistoryRepository = new TestPublishingHistoryRepository();

        using var factory = BuildFactoryWithSeeds(
            [publishSeed, deliverySeed],
            publishingHistoryRepository);
        using var client = factory.CreateClient();
        using var publishRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{tenantBContent.Id}/versions/{version.Id}/publish",
            publishSeed,
            BuildPublishRequest());

        var publishResponse = await client.SendAsync(publishRequest);
        var publishRawBody = await publishResponse.Content.ReadAsStringAsync();
        var deliveryResponse = await SendDeliveryLookupAsync(client, deliverySeed, tenantBContent.ApplicationId, "production", "Published");
        var deliveryRawBody = await deliveryResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, publishResponse.StatusCode);
        Assert.Empty(publishingHistoryRepository.Records);
        Assert.Equal(HttpStatusCode.NotFound, deliveryResponse.StatusCode);
        Assert.DoesNotContain(tenantBId.ToString(), publishRawBody, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantBContent.Id.ToString(), publishRawBody, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantBId.ToString(), deliveryRawBody, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantBContent.ApplicationId.ToString(), deliveryRawBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Delivery_api_returns_safe_not_found_for_wrong_scope_after_publish()
    {
        var tenantId = Guid.NewGuid();
        var publishSeed = TestIdentitySeed.Create(AdoptaPermissionKeys.AuthoringPublish, tenantId);
        var deliverySeed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead, tenantId);
        var otherTenantDeliverySeed = TestIdentitySeed.Create(AdoptaPermissionKeys.RuntimeDeliveryRead, Guid.NewGuid());
        var content = await SeedContentAsync(tenantId, ContentLifecycleState.Approved);
        var version = Assert.Single(content.Versions);
        var publishingHistoryRepository = new TestPublishingHistoryRepository();

        using var factory = BuildFactoryWithSeeds(
            [publishSeed, deliverySeed, otherTenantDeliverySeed],
            publishingHistoryRepository);
        using var client = factory.CreateClient();
        using var publishRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            $"/authoring/content/{content.Id}/versions/{version.Id}/publish",
            publishSeed,
            BuildPublishRequest());

        var publishResponse = await client.SendAsync(publishRequest);
        var wrongApplication = await SendDeliveryLookupAsync(client, deliverySeed, Guid.NewGuid(), "production", "Published");
        var wrongEnvironment = await SendDeliveryLookupAsync(client, deliverySeed, content.ApplicationId, "test", "Published");
        var wrongChannel = await SendDeliveryLookupAsync(client, deliverySeed, content.ApplicationId, "production", "Preview");
        var wrongTenant = await SendDeliveryLookupAsync(client, otherTenantDeliverySeed, content.ApplicationId, "production", "Published");

        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, wrongApplication.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, wrongEnvironment.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, wrongChannel.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, wrongTenant.StatusCode);
    }

    private static async Task<HttpResponseMessage> SendDeliveryLookupAsync(
        HttpClient client,
        TestIdentitySeed seed,
        Guid applicationId,
        string environment,
        string channel)
    {
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"/runtime/delivery/bundles/{applicationId}?environment={environment}&channel={channel}",
            seed);

        return await client.SendAsync(request);
    }

    private static WebApplicationFactory<Program> BuildFactoryWithSeeds(
        IReadOnlyCollection<TestIdentitySeed> seeds,
        IAuthoredContentPublishingHistoryRepository publishingHistoryRepository)
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
                    services.RemoveAll<IAuthoredContentPublishingHistoryRepository>();
                    services.AddSingleton(publishingHistoryRepository);
                });
            });
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

    private static PublishAuthoredContentRequest BuildPublishRequest()
    {
        return new PublishAuthoredContentRequest(
            "production",
            DeliveryChannel.Published,
            null);
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
            $"billing.submit.{Guid.NewGuid():N}",
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

    private static void AssertNoSensitiveMarkers(string value)
    {
        Assert.DoesNotContain("Bearer", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AccountKey", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HMRC", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tax", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("property", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", value, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record TestIdentitySeed(
        string ExternalTenantId,
        string ApplicationId,
        string SubjectId,
        Guid InternalTenantId,
        string PermissionKey)
    {
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

    private sealed class TestPublishingHistoryRepository : IAuthoredContentPublishingHistoryRepository
    {
        private readonly List<AuthoredContentPublishingAuditRecord> _records = [];

        public IReadOnlyCollection<AuthoredContentPublishingAuditRecord> Records => _records;

        public Task AddAsync(
            AuthoredContentPublishingAuditRecord auditRecord,
            CancellationToken cancellationToken = default)
        {
            _records.Add(auditRecord);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<AuthoredContentPublishingAuditRecord>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<AuthoredContentPublishingAuditRecord>>(_records.ToArray());
        }
    }
}
