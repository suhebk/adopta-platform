using Adopta.Domain.Identity;

namespace Adopta.Infrastructure.Identity;

public sealed record AuthenticatedUserMappingRecord(
    Guid TenantId,
    string ExternalSubjectId,
    AdoptionUser User);
