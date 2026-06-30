namespace Adopta.Web.Studio;

public static class StudioReadApiActivationValidator
{
    public static StudioApiClientOptions ReadApiOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(StudioApiClientOptions.SectionName);

        return new StudioApiClientOptions
        {
            Enabled = bool.TryParse(section[nameof(StudioApiClientOptions.Enabled)], out var enabled)
                && enabled,
            BaseAddress = section[nameof(StudioApiClientOptions.BaseAddress)] ?? string.Empty
        };
    }

    public static StudioReadApiActivationValidationResult Validate(
        StudioApiClientOptions apiOptions,
        StudioWebAuthenticationOptions authenticationOptions,
        StudioApiTokenAcquisitionOptions tokenAcquisitionOptions)
    {
        if (!apiOptions.Enabled)
        {
            return StudioReadApiActivationValidationResult.Disabled();
        }

        var issues = new List<StudioReadApiActivationValidationIssue>();

        if (!apiOptions.HasConfiguredBaseAddress)
        {
            issues.Add(Issue(
                "studio_api_base_address_required",
                "Studio read API activation is incomplete."));
        }

        if (!authenticationOptions.Enabled || !authenticationOptions.HasCompleteConfiguration)
        {
            issues.Add(Issue(
                "studio_web_authentication_required",
                "Studio read API activation is incomplete."));
        }

        if (!tokenAcquisitionOptions.Enabled || !tokenAcquisitionOptions.HasConfiguredScopes)
        {
            issues.Add(Issue(
                "studio_api_access_required",
                "Studio read API activation is incomplete."));
        }

        return issues.Count == 0
            ? StudioReadApiActivationValidationResult.Active()
            : StudioReadApiActivationValidationResult.Invalid(issues);
    }

    public static StudioReadApiActivationValidationResult Validate(IConfiguration configuration) =>
        Validate(
            ReadApiOptions(configuration),
            StudioWebAuthenticationConfigurationValidator.ReadAuthenticationOptions(configuration),
            StudioWebAuthenticationConfigurationValidator.ReadTokenAcquisitionOptions(configuration));

    private static StudioReadApiActivationValidationIssue Issue(string code, string message) =>
        new(code, message);
}
