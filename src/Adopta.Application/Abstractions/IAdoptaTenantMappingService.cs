using Adopta.Application.Identity;

namespace Adopta.Application.Abstractions;

public interface IAdoptaTenantMappingService
{
    AdoptaTenantMappingResult MapTenant(string externalTenantId, string applicationId);
}
