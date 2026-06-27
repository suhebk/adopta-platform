using Adopta.Domain.Common;

namespace Adopta.Infrastructure.Tenancy;

public sealed class InMemoryTenantScopedStore<T>
    where T : ITenantScopedEntity
{
    private readonly List<T> _items = [];

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public IReadOnlyCollection<T> ListForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        return _items
            .Where(item => item.TenantId == tenantId)
            .ToArray();
    }
}
