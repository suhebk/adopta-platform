using Adopta.Domain.Common;

namespace Adopta.Domain.Tenancy;

public sealed class TenantApplication : TenantScopedEntity
{
    public TenantApplication(Guid id, Guid tenantId, string name, Uri allowedOrigin)
        : base(tenantId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Application id is required.", nameof(id));
        }

        Id = id;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Application name is required.", nameof(name))
            : name.Trim();
        AllowedOrigin = allowedOrigin ?? throw new ArgumentNullException(nameof(allowedOrigin));
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }

    public string Name { get; }

    public Uri AllowedOrigin { get; }

    public DateTimeOffset CreatedAtUtc { get; }
}
