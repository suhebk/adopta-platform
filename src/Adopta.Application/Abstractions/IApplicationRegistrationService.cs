using Adopta.Domain.Tenancy;

namespace Adopta.Application.Abstractions;

public interface IApplicationRegistrationService
{
    Task<TenantApplication> RegisterAsync(
        string name,
        Uri allowedOrigin,
        CancellationToken cancellationToken = default);
}
