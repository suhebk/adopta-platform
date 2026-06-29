namespace Adopta.Domain.Authoring;

public sealed class AuthoredContentVersion
{
    public AuthoredContentVersion(
        Guid id,
        string version,
        ContentLifecycleState lifecycleState,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Version = version;
        LifecycleState = lifecycleState;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public string Version { get; }

    public ContentLifecycleState LifecycleState { get; }

    public DateTimeOffset CreatedAtUtc { get; }
}
