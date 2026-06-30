using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public sealed record StudioContentListRequest(Guid? ApplicationId = null);

public sealed record StudioContentGetByIdRequest(Guid ContentId);

public sealed record StudioContentCreateDraftRequest(
    Guid ApplicationId,
    string Title,
    string ContentKey,
    RuntimeContentType? ContentType,
    string Version);

public sealed record StudioContentUpdateDraftRequest(
    Guid ContentId,
    Guid ApplicationId,
    string Title,
    string ContentKey,
    RuntimeContentType? ContentType,
    string Version);

public sealed record StudioWorkflowActionRequest(
    Guid ContentId,
    Guid VersionId);
