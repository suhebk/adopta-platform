using Adopta.Domain.Common;

namespace Adopta.Domain.Authoring;

public sealed class AuthoredContentItem : TenantScopedEntity
{
    private readonly List<AuthoredContentVersion> _versions;

    private AuthoredContentItem(
        Guid id,
        Guid tenantId,
        Guid applicationId,
        AuthoredContentType contentType,
        string contentKey,
        string title)
        : this(id, tenantId, applicationId, contentType, contentKey, title, null)
    {
    }

    public AuthoredContentItem(
        Guid id,
        Guid tenantId,
        Guid applicationId,
        AuthoredContentType contentType,
        string contentKey,
        string title,
        IReadOnlyCollection<AuthoredContentVersion>? versions = null)
        : base(tenantId)
    {
        Id = id;
        ApplicationId = applicationId;
        ContentType = contentType;
        ContentKey = contentKey;
        Title = title;
        _versions = versions?.ToList() ?? [];
    }

    public Guid Id { get; }

    public Guid ApplicationId { get; }

    public AuthoredContentType ContentType { get; }

    public string ContentKey { get; }

    public string Title { get; }

    public IReadOnlyCollection<AuthoredContentVersion> Versions => _versions.AsReadOnly();
}
