namespace Adopta.Infrastructure.Audit;

public sealed record InMemorySecurityAuditEvent(
    DateTimeOffset OccurredAtUtc,
    string Action,
    string Outcome,
    Guid? TenantId,
    string? FailureCategory);
