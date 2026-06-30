namespace Adopta.Web.Studio;

public enum StudioContentClientStatus
{
    Success = 0,
    Unauthorized = 1,
    Forbidden = 2,
    NotFound = 3,
    InvalidResponse = 4,
    Unavailable = 5,
    UnexpectedError = 6
}

public sealed record StudioContentClientResult<T>(
    StudioContentClientStatus Status,
    T? Value,
    string SafeMessage)
{
    public bool Succeeded => Status == StudioContentClientStatus.Success;

    public static StudioContentClientResult<T> Success(T value, string? safeMessage = null) =>
        new(StudioContentClientStatus.Success, value, safeMessage ?? "Studio content loaded.");

    public static StudioContentClientResult<T> Unauthorized() =>
        Failed(StudioContentClientStatus.Unauthorized, "Authentication is required to view Studio content.");

    public static StudioContentClientResult<T> Forbidden() =>
        Failed(StudioContentClientStatus.Forbidden, "You do not have access to Studio content.");

    public static StudioContentClientResult<T> NotFound() =>
        Failed(StudioContentClientStatus.NotFound, "Studio content was not found.");

    public static StudioContentClientResult<T> InvalidResponse() =>
        Failed(StudioContentClientStatus.InvalidResponse, "Studio content could not be loaded.");

    public static StudioContentClientResult<T> Unavailable() =>
        Failed(StudioContentClientStatus.Unavailable, "Studio content is temporarily unavailable.");

    public static StudioContentClientResult<T> UnexpectedError() =>
        Failed(StudioContentClientStatus.UnexpectedError, "Studio content could not be loaded.");

    public static StudioContentClientResult<T> Failed(
        StudioContentClientStatus status,
        string safeMessage) =>
        new(status, default, safeMessage);
}
