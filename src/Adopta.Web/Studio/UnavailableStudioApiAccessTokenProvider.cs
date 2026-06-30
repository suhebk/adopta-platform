namespace Adopta.Web.Studio;

public sealed class UnavailableStudioApiAccessTokenProvider : IStudioApiAccessTokenProvider
{
    public Task<StudioApiAccessTokenResult> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(StudioApiAccessTokenResult.Unavailable());
    }
}
