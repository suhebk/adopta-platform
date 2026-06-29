using Adopta.Application.Runtime;

namespace Adopta.Application.Authoring;

public enum AuthoredContentPublishStatus
{
    Succeeded = 0,
    InvalidRequest = 1,
    NotFound = 2,
    ValidationFailed = 3
}

public sealed record AuthoredContentPublishCommand(
    Guid TenantId,
    Guid ContentId,
    Guid VersionId,
    Guid ActorUserId,
    string Environment,
    DeliveryChannel Channel,
    DateTimeOffset RequestedAtUtc);

public sealed record AuthoredContentPublishResult(
    AuthoredContentPublishStatus Status,
    DeliveryBundle? Bundle,
    AuthoredContentPublishingAuditRecord? AuditRecord,
    IReadOnlyCollection<AuthoredContentValidationIssue> Issues)
{
    public bool IsSuccess => Status == AuthoredContentPublishStatus.Succeeded;

    public static AuthoredContentPublishResult Succeeded(
        DeliveryBundle bundle,
        AuthoredContentPublishingAuditRecord auditRecord)
    {
        return new AuthoredContentPublishResult(
            AuthoredContentPublishStatus.Succeeded,
            bundle,
            auditRecord,
            []);
    }

    public static AuthoredContentPublishResult Failed(
        AuthoredContentPublishStatus status,
        IReadOnlyCollection<AuthoredContentValidationIssue> issues)
    {
        return new AuthoredContentPublishResult(
            status,
            null,
            null,
            issues);
    }
}
