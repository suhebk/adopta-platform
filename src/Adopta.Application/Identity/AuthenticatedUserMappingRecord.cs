using Adopta.Domain.Identity;

namespace Adopta.Application.Identity;

public sealed record AuthenticatedUserMappingRecord(
    Guid TenantId,
    string ExternalSubjectId,
    AdoptionUser User);
