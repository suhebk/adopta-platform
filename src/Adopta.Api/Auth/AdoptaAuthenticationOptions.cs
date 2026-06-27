namespace Adopta.Api.Auth;

public sealed class AdoptaAuthenticationOptions
{
    public const string SectionName = "Authentication:MicrosoftEntra";

    public string Authority { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;
}
