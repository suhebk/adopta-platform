using Adopta.Domain.Common;

namespace Adopta.Domain.Identity;

public sealed class Role : TenantScopedEntity
{
    private readonly List<Permission> _permissions = [];

    public Role(Guid id, Guid tenantId, string name)
        : base(tenantId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(id));
        }

        Id = id;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Role name is required.", nameof(name))
            : name.Trim();
    }

    public Guid Id { get; }

    public string Name { get; }

    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    public void Grant(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (_permissions.All(existing => existing.Key != permission.Key))
        {
            _permissions.Add(permission);
        }
    }
}
