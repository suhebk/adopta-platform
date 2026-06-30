namespace Adopta.Web.Studio;

public enum StudioApiAccessTokenStatus
{
    Available = 0,
    Unavailable = 1,
    Invalid = 2
}

public sealed record StudioApiAccessTokenResult(
    StudioApiAccessTokenStatus Status,
    string? AccessToken,
    string SafeMessage)
{
    public bool HasAccessToken =>
        Status == StudioApiAccessTokenStatus.Available
        && !string.IsNullOrWhiteSpace(AccessToken);

    public static StudioApiAccessTokenResult Available(string accessToken)
    {
        return string.IsNullOrWhiteSpace(accessToken)
            ? Invalid()
            : new StudioApiAccessTokenResult(
                StudioApiAccessTokenStatus.Available,
                accessToken,
                "Studio API access is available.");
    }

    public static StudioApiAccessTokenResult Unavailable() =>
        new(
            StudioApiAccessTokenStatus.Unavailable,
            null,
            "Studio API access is not configured.");

    public static StudioApiAccessTokenResult Invalid() =>
        new(
            StudioApiAccessTokenStatus.Invalid,
            null,
            "Studio API access is not available.");
}
