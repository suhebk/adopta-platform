namespace Adopta.Web.Studio;

public static class StudioWebAuthenticationConfigurationValidator
{
    public const string SafeConfigurationErrorMessage =
        "Studio Web authentication configuration is invalid.";

    public static StudioWebAuthenticationOptions ReadAuthenticationOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(StudioWebAuthenticationOptions.SectionName);

        return new StudioWebAuthenticationOptions
        {
            Enabled = bool.TryParse(section[nameof(StudioWebAuthenticationOptions.Enabled)], out var enabled)
                && enabled,
            Authority = section[nameof(StudioWebAuthenticationOptions.Authority)] ?? string.Empty,
            ClientId = section[nameof(StudioWebAuthenticationOptions.ClientId)] ?? string.Empty,
            CallbackPath = section[nameof(StudioWebAuthenticationOptions.CallbackPath)]
                ?? new StudioWebAuthenticationOptions().CallbackPath
        };
    }

    public static StudioApiTokenAcquisitionOptions ReadTokenAcquisitionOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(StudioApiTokenAcquisitionOptions.SectionName);

        return new StudioApiTokenAcquisitionOptions
        {
            Enabled = bool.TryParse(section[nameof(StudioApiTokenAcquisitionOptions.Enabled)], out var enabled)
                && enabled,
            Scopes = section
                .GetSection(nameof(StudioApiTokenAcquisitionOptions.Scopes))
                .GetChildren()
                .Select(scope => scope.Value ?? string.Empty)
                .Where(scope => !string.IsNullOrWhiteSpace(scope))
                .ToArray()
        };
    }

    public static StudioWebAuthenticationValidationResult Validate(
        StudioWebAuthenticationOptions authenticationOptions,
        StudioApiTokenAcquisitionOptions tokenAcquisitionOptions)
    {
        var issues = new List<StudioWebAuthenticationValidationIssue>();

        if (authenticationOptions.Enabled && !authenticationOptions.HasCompleteConfiguration)
        {
            issues.Add(Issue(
                "studio_web_authentication_incomplete",
                "Studio Web authentication is incomplete."));
        }

        if (tokenAcquisitionOptions.Enabled && !authenticationOptions.Enabled)
        {
            issues.Add(Issue(
                "studio_api_token_requires_web_authentication",
                "Studio API access requires Web authentication."));
        }

        if (tokenAcquisitionOptions.Enabled && !tokenAcquisitionOptions.HasConfiguredScopes)
        {
            issues.Add(Issue(
                "studio_api_scopes_missing",
                "Studio API scopes are incomplete."));
        }

        return issues.Count == 0
            ? StudioWebAuthenticationValidationResult.Valid()
            : StudioWebAuthenticationValidationResult.Failed(issues);
    }

    public static StudioWebAuthenticationValidationResult Validate(IConfiguration configuration) =>
        Validate(ReadAuthenticationOptions(configuration), ReadTokenAcquisitionOptions(configuration));

    private static StudioWebAuthenticationValidationIssue Issue(string code, string message) =>
        new(code, message);
}
