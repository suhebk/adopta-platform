namespace Adopta.Application.Abstractions;

public interface IAdoptionTenantContext
{
    Guid TenantId { get; }

    string? ExternalTenantId { get; }

    bool HasTenant { get; }
}
