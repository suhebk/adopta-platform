using System.Text.Json;
using Adopta.Application.Runtime;

namespace Adopta.UnitTests;

public sealed class RuntimeDeliveryBundleContractTests
{
    [Fact]
    public void Valid_lookup_request_has_no_validation_issues()
    {
        var request = new DeliveryBundleLookupRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "production",
            DeliveryChannel.Published);

        Assert.Empty(request.Validate());
    }

    [Fact]
    public void Invalid_lookup_request_returns_typed_invalid_result()
    {
        var request = new DeliveryBundleLookupRequest(
            Guid.Empty,
            Guid.Empty,
            "",
            DeliveryChannel.Published);

        var result = DeliveryBundleLookupResult.InvalidRequest(request.Validate());

        Assert.Equal(DeliveryBundleLookupStatus.InvalidRequest, result.Status);
        Assert.Contains(result.Issues, issue => issue.Code == "invalid_delivery_bundle_lookup_request");
    }

    [Fact]
    public void Valid_fixture_validates_successfully()
    {
        var bundle = ReadFixture("runtime-delivery-bundle.valid.json");

        var issues = RuntimeContentValidator.ValidateBundle(bundle);

        Assert.Empty(issues);
    }

    [Fact]
    public void Invalid_fixture_returns_typed_validation_issues()
    {
        var bundle = ReadFixture("runtime-delivery-bundle.invalid.json");

        var issues = RuntimeContentValidator.ValidateBundle(bundle);

        Assert.Contains(issues, issue => issue.Code == "invalid_content_bundle");
        Assert.Contains(issues, issue => issue.Code == "invalid_content_item");
        Assert.Contains(issues, issue => issue.Code == "invalid_anchor_descriptor");
        Assert.Contains(issues, issue => issue.Code == "invalid_targeting_placeholder");
    }

    private static RuntimeContentBundle ReadFixture(string fileName)
    {
        var fixturePath = Path.Combine(FindRepositoryRoot(), "tests", "Adopta.UnitTests", "Fixtures", fileName);
        var json = File.ReadAllText(fixturePath);
        var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var item = root.GetProperty("items")[0];
        var anchor = item.GetProperty("anchor");
        var targeting = item.GetProperty("targeting");

        return new RuntimeContentBundle(
            root.GetProperty("bundleId").GetString() ?? "",
            root.GetProperty("tenantId").GetString() ?? "",
            root.GetProperty("applicationId").GetString() ?? "",
            root.GetProperty("environment").GetString() ?? "",
            root.GetProperty("channel").GetString() ?? "",
            root.GetProperty("version").GetString() ?? "",
            root.GetProperty("generatedAtUtc").GetDateTimeOffset(),
            [
                new RuntimeContentItem(
                    item.GetProperty("id").GetString() ?? "",
                    RuntimeContentType.Tooltip,
                    item.GetProperty("version").GetString() ?? "",
                    item.GetProperty("title").GetString() ?? "",
                    item.TryGetProperty("body", out var body) ? body.GetString() : null,
                    new RuntimeAnchorDescriptor(
                        anchor.GetProperty("strategy").GetString() ?? "",
                        anchor.GetProperty("value").GetString() ?? ""),
                    new RuntimeTargetingPlaceholder(
                        targeting.GetProperty("mode").GetString() ?? "",
                        [],
                        []))
            ]);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Adopta.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root could not be found.");
    }
}
