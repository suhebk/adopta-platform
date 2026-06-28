namespace Adopta.Application.Audit;

public sealed record SecurityAuditEventRecord(
    Guid TenantId,
    DateTimeOffset OccurredAtUtc,
    string Action,
    string Outcome,
    string? FailureCategory);
