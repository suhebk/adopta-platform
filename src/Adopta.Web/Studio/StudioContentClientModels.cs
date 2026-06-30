namespace Adopta.Web.Studio;

public sealed record StudioContentListRequest(Guid? ApplicationId = null);

public sealed record StudioContentGetByIdRequest(Guid ContentId);
