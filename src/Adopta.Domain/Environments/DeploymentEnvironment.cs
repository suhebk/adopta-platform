using Adopta.Domain.Common;

namespace Adopta.Domain.Environments;

public sealed class DeploymentEnvironment : TenantScopedEntity
{
    public DeploymentEnvironment(Guid id, Guid tenantId, DeploymentEnvironmentKind kind, string displayName)
        : base(tenantId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Environment id is required.", nameof(id));
        }

        Id = id;
        Kind = kind;
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Display name is required.", nameof(displayName))
            : displayName.Trim();
    }

    public Guid Id { get; }

    public DeploymentEnvironmentKind Kind { get; }

    public string DisplayName { get; }
}
