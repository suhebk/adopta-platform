namespace Adopta.Infrastructure.Tenancy;

public sealed record AdoptaTenantMappingRecord(
    string ExternalTenantId,
    string ApplicationId,
    Guid TenantId);
