namespace Adopta.Application.Identity;

public sealed record TenantMappingRecord(
    Guid TenantId,
    string ExternalTenantId,
    string ApplicationId);
