using Adopta.Domain.Identity;

namespace Adopta.Infrastructure.Identity;

public sealed class InMemoryAuthenticatedUserMappingStore
{
    private readonly Lock _gate = new();
    private readonly List<AuthenticatedUserMappingRecord> _records = [];

    public void Add(Guid tenantId, string externalSubjectId, AdoptionUser user)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(externalSubjectId))
        {
            throw new ArgumentException("External subject id is required.", nameof(externalSubjectId));
        }

        ArgumentNullException.ThrowIfNull(user);

        lock (_gate)
        {
            _records.Add(new AuthenticatedUserMappingRecord(
                tenantId,
                externalSubjectId.Trim(),
                user));
        }
    }

    public IReadOnlyCollection<AuthenticatedUserMappingRecord> Find(Guid tenantId, string externalSubjectId)
    {
        lock (_gate)
        {
            return _records
                .Where(record =>
                    record.TenantId == tenantId
                    && record.ExternalSubjectId.Equals(externalSubjectId, StringComparison.Ordinal))
                .ToArray();
        }
    }
}
