namespace Adopta.Web.Studio;

public interface IStudioApiAccessTokenProvider
{
    Task<StudioApiAccessTokenResult> GetAccessTokenAsync(CancellationToken cancellationToken);
}
