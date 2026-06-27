namespace Adopta.Infrastructure.Tenancy;

public sealed class InMemoryAdoptaTenantMappingStore
{
    private readonly Lock _gate = new();
    private readonly List<AdoptaTenantMappingRecord> _records = [];

    public void Add(string externalTenantId, string applicationId, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(externalTenantId))
        {
            throw new ArgumentException("External tenant id is required.", nameof(externalTenantId));
        }

        if (string.IsNullOrWhiteSpace(applicationId))
        {
            throw new ArgumentException("Application id is required.", nameof(applicationId));
        }

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        lock (_gate)
        {
            _records.Add(new AdoptaTenantMappingRecord(
                externalTenantId.Trim(),
                applicationId.Trim(),
                tenantId));
        }
    }

    public IReadOnlyCollection<AdoptaTenantMappingRecord> Find(string externalTenantId, string applicationId)
    {
        lock (_gate)
        {
            return _records
                .Where(record =>
                    record.ExternalTenantId.Equals(externalTenantId, StringComparison.Ordinal)
                    && record.ApplicationId.Equals(applicationId, StringComparison.Ordinal))
                .ToArray();
        }
    }
}
