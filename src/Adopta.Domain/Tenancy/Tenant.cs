namespace Adopta.Domain.Tenancy;

public sealed class Tenant
{
    public Tenant(Guid id, string name, string primaryDomain, string dataRegion)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(id));
        }

        Id = id;
        Name = RequireText(name, nameof(name));
        PrimaryDomain = RequireText(primaryDomain, nameof(primaryDomain));
        DataRegion = RequireText(dataRegion, nameof(dataRegion));
        CreatedAtUtc = DateTimeOffset.UtcNow;
        Status = TenantStatus.Active;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string PrimaryDomain { get; }

    public string DataRegion { get; }

    public TenantStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public void Suspend() => Status = TenantStatus.Suspended;

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value.Trim();
    }
}
