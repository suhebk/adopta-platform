namespace Adopta.Web.Studio;

public sealed class StudioWebAuthenticationOptions
{
    public const string SectionName = "Authentication:StudioWeb";

    public bool Enabled { get; set; }

    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/signin-oidc";

    public bool HasCompleteConfiguration =>
        !string.IsNullOrWhiteSpace(ClientId)
        && Guid.TryParse(ClientId, out var parsedClientId)
        && parsedClientId != Guid.Empty
        && Uri.TryCreate(Authority, UriKind.Absolute, out var authorityUri)
        && authorityUri.Scheme == Uri.UriSchemeHttps
        && !string.IsNullOrWhiteSpace(CallbackPath)
        && CallbackPath.StartsWith('/')
        && CallbackPath.IndexOfAny(['\r', '\n']) < 0;
}
