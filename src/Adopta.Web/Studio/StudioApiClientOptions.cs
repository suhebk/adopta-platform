namespace Adopta.Web.Studio;

public sealed class StudioApiClientOptions
{
    public const string SectionName = "StudioApi";

    public bool Enabled { get; set; }

    public string BaseAddress { get; set; } = string.Empty;

    public bool HasConfiguredBaseAddress =>
        Uri.TryCreate(BaseAddress, UriKind.Absolute, out var uri)
        && uri.Scheme is "https";
}
