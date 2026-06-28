namespace Adopta.Application.Runtime;

public sealed record RuntimeContentBundle(
    string BundleId,
    string TenantId,
    string ApplicationId,
    string Environment,
    string Channel,
    string Version,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyCollection<RuntimeContentItem> Items);

