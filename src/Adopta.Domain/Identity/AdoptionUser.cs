using Adopta.Domain.Common;

namespace Adopta.Domain.Identity;

public sealed class AdoptionUser : TenantScopedEntity
{
    private readonly List<Role> _roles = [];

    public AdoptionUser(Guid id, Guid tenantId, string externalUserId, string displayName)
        : base(tenantId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(id));
        }

        Id = id;
        ExternalUserId = string.IsNullOrWhiteSpace(externalUserId)
            ? throw new ArgumentException("External user id is required.", nameof(externalUserId))
            : externalUserId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Display name is required.", nameof(displayName))
            : displayName.Trim();
    }

    public Guid Id { get; }

    public string ExternalUserId { get; }

    public string DisplayName { get; }

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (role.TenantId != TenantId)
        {
            throw new InvalidOperationException("Cannot assign a role from another tenant.");
        }

        if (_roles.All(existing => existing.Id != role.Id))
        {
            _roles.Add(role);
        }
    }
}
