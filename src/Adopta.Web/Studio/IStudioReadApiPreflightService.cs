namespace Adopta.Web.Studio;

public interface IStudioReadApiPreflightService
{
    Task<StudioReadApiPreflightResult> RunAsync(CancellationToken cancellationToken);
}
