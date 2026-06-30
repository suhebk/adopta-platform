using Adopta.Application.Runtime;

namespace Adopta.Web.Studio;

public sealed class StudioReadApiPreflightService : IStudioReadApiPreflightService
{
    private readonly IConfiguration configuration;

    public StudioReadApiPreflightService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<StudioReadApiPreflightResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var apiOptions = StudioReadApiActivationValidator.ReadApiOptions(configuration);
        var authenticationOptions =
            StudioWebAuthenticationConfigurationValidator.ReadAuthenticationOptions(configuration);
        var tokenAcquisitionOptions =
            StudioWebAuthenticationConfigurationValidator.ReadTokenAcquisitionOptions(configuration);
        var activation = StudioReadApiActivationValidator.Validate(
            apiOptions,
            authenticationOptions,
            tokenAcquisitionOptions);

        var checks = new List<StudioReadApiPreflightCheck>
        {
            BuildDisabledByDefaultCheck(apiOptions),
            BuildBaseAddressCheck(apiOptions),
            BuildAuthenticationCheck(apiOptions, authenticationOptions),
            BuildAccessAcquisitionCheck(apiOptions, tokenAcquisitionOptions),
            BuildStaticCheck(
                StudioReadApiPreflightCheckCodes.RequestBoundaryHandlerRegistered,
                "Studio API request boundary is available."),
            BuildTenantBoundaryCheck(),
            BuildTestShortcutCheck(),
            BuildReadOnlyCheck(await VerifyReadClientWriteMethodsUnavailableAsync(cancellationToken)),
            BuildLocalFallbackCheck(activation),
            BuildActivationFailClosedCheck(activation)
        };

        return new StudioReadApiPreflightResult(
            ResolveStatus(activation, checks),
            checks);
    }

    private static StudioReadApiPreflightCheck BuildDisabledByDefaultCheck(
        StudioApiClientOptions apiOptions) =>
        apiOptions.Enabled
            ? Check(
                StudioReadApiPreflightCheckCodes.StudioApiDisabledByDefault,
                StudioReadApiPreflightCheckStatus.NotApplicable,
                "Studio API activation was explicitly requested.")
            : Check(
                StudioReadApiPreflightCheckCodes.StudioApiDisabledByDefault,
                StudioReadApiPreflightCheckStatus.Passed,
                "Studio API activation is disabled by default.");

    private static StudioReadApiPreflightCheck BuildBaseAddressCheck(
        StudioApiClientOptions apiOptions)
    {
        if (!apiOptions.Enabled)
        {
            return Check(
                StudioReadApiPreflightCheckCodes.StudioApiBaseAddressConfigured,
                StudioReadApiPreflightCheckStatus.NotApplicable,
                "Studio API endpoint configuration is not required while activation is disabled.");
        }

        return apiOptions.HasConfiguredBaseAddress
            ? Check(
                StudioReadApiPreflightCheckCodes.StudioApiBaseAddressConfigured,
                StudioReadApiPreflightCheckStatus.Passed,
                "A secure Studio API endpoint is configured.")
            : Check(
                StudioReadApiPreflightCheckCodes.StudioApiBaseAddressConfigured,
                StudioReadApiPreflightCheckStatus.Failed,
                "A secure Studio API endpoint is required before activation.");
    }

    private static StudioReadApiPreflightCheck BuildAuthenticationCheck(
        StudioApiClientOptions apiOptions,
        StudioWebAuthenticationOptions authenticationOptions)
    {
        if (!apiOptions.Enabled)
        {
            return Check(
                StudioReadApiPreflightCheckCodes.StudioWebAuthenticationConfigured,
                StudioReadApiPreflightCheckStatus.NotApplicable,
                "Studio Web authentication configuration is not required while activation is disabled.");
        }

        return authenticationOptions.Enabled && authenticationOptions.HasCompleteConfiguration
            ? Check(
                StudioReadApiPreflightCheckCodes.StudioWebAuthenticationConfigured,
                StudioReadApiPreflightCheckStatus.Passed,
                "Studio Web authentication prerequisites are configured.")
            : Check(
                StudioReadApiPreflightCheckCodes.StudioWebAuthenticationConfigured,
                StudioReadApiPreflightCheckStatus.Failed,
                "Studio Web authentication prerequisites are required before activation.");
    }

    private static StudioReadApiPreflightCheck BuildAccessAcquisitionCheck(
        StudioApiClientOptions apiOptions,
        StudioApiTokenAcquisitionOptions tokenAcquisitionOptions)
    {
        if (!apiOptions.Enabled)
        {
            return Check(
                StudioReadApiPreflightCheckCodes.StudioApiTokenAcquisitionConfigured,
                StudioReadApiPreflightCheckStatus.NotApplicable,
                "Studio API access configuration is not required while activation is disabled.");
        }

        return tokenAcquisitionOptions.Enabled && tokenAcquisitionOptions.HasConfiguredScopes
            ? Check(
                StudioReadApiPreflightCheckCodes.StudioApiTokenAcquisitionConfigured,
                StudioReadApiPreflightCheckStatus.Passed,
                "Studio API access prerequisites are configured.")
            : Check(
                StudioReadApiPreflightCheckCodes.StudioApiTokenAcquisitionConfigured,
                StudioReadApiPreflightCheckStatus.Failed,
                "Studio API access prerequisites are required before activation.");
    }

    private static StudioReadApiPreflightCheck BuildTenantBoundaryCheck()
    {
        var passed = StudioApiRequestBoundaryHandler.IsProhibitedHeader(
            StudioApiRequestBoundaryHandler.TenantHeaderName);

        return Check(
            StudioReadApiPreflightCheckCodes.TenantHeaderNotClientSupplied,
            passed ? StudioReadApiPreflightCheckStatus.Passed : StudioReadApiPreflightCheckStatus.Failed,
            passed
                ? "Tenant context remains server-side."
                : "Tenant context must remain server-side.");
    }

    private static StudioReadApiPreflightCheck BuildTestShortcutCheck()
    {
        var passed = StudioApiRequestBoundaryHandler.IsProhibitedHeader(
            string.Concat(StudioApiRequestBoundaryHandler.TestHeaderPrefix, "Authenticated"));

        return Check(
            StudioReadApiPreflightCheckCodes.TestHeadersNotProductionShortcuts,
            passed ? StudioReadApiPreflightCheckStatus.Passed : StudioReadApiPreflightCheckStatus.Failed,
            passed
                ? "Test-only shortcuts remain blocked in production Web paths."
                : "Test-only shortcuts must remain blocked in production Web paths.");
    }

    private static StudioReadApiPreflightCheck BuildReadOnlyCheck(bool readOnlyPosturePassed) =>
        Check(
            StudioReadApiPreflightCheckCodes.ReadClientIsReadOnly,
            readOnlyPosturePassed
                ? StudioReadApiPreflightCheckStatus.Passed
                : StudioReadApiPreflightCheckStatus.Failed,
            readOnlyPosturePassed
                ? "Live read client exposes read-only operations for activation."
                : "Live read client must expose read-only operations for activation.");

    private static StudioReadApiPreflightCheck BuildLocalFallbackCheck(
        StudioReadApiActivationValidationResult activation) =>
        activation.CanActivate
            ? Check(
                StudioReadApiPreflightCheckCodes.LocalFallbackAvailable,
                StudioReadApiPreflightCheckStatus.NotApplicable,
                "Local fallback is reserved for disabled or invalid activation.")
            : Check(
                StudioReadApiPreflightCheckCodes.LocalFallbackAvailable,
                StudioReadApiPreflightCheckStatus.Passed,
                "Local Studio content fallback remains available.");

    private static StudioReadApiPreflightCheck BuildActivationFailClosedCheck(
        StudioReadApiActivationValidationResult activation) =>
        activation.Status == StudioReadApiActivationStatus.Active
            ? Check(
                StudioReadApiPreflightCheckCodes.ActivationFailsClosed,
                StudioReadApiPreflightCheckStatus.Passed,
                "Activation can proceed only because all prerequisites passed.")
            : Check(
                StudioReadApiPreflightCheckCodes.ActivationFailsClosed,
                StudioReadApiPreflightCheckStatus.Passed,
                "Activation remains disabled or fails closed unless every prerequisite passes.");

    private static StudioReadApiPreflightCheck BuildStaticCheck(string code, string message) =>
        Check(code, StudioReadApiPreflightCheckStatus.Passed, message);

    private static StudioReadApiPreflightCheck Check(
        string code,
        StudioReadApiPreflightCheckStatus status,
        string message) =>
        new(code, status, message);

    private static StudioReadApiPreflightStatus ResolveStatus(
        StudioReadApiActivationValidationResult activation,
        IReadOnlyCollection<StudioReadApiPreflightCheck> checks)
    {
        if (checks.Any(check => check.Status == StudioReadApiPreflightCheckStatus.Failed))
        {
            return StudioReadApiPreflightStatus.Invalid;
        }

        return activation.Status switch
        {
            StudioReadApiActivationStatus.Active => StudioReadApiPreflightStatus.Ready,
            StudioReadApiActivationStatus.Disabled => StudioReadApiPreflightStatus.Disabled,
            _ => StudioReadApiPreflightStatus.Invalid
        };
    }

    private static async Task<bool> VerifyReadClientWriteMethodsUnavailableAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient(new NoNetworkHandler())
            {
                BaseAddress = new Uri("https://localhost")
            };
            var client = new StudioAuthoringReadApiClient(httpClient);

            var create = await client.CreateDraftAsync(
                new StudioContentCreateDraftRequest(
                    Guid.NewGuid(),
                    "Guidance title",
                    "guidance.title",
                    RuntimeContentType.Tooltip,
                    "1.0.0"),
                cancellationToken);
            var update = await client.UpdateDraftAsync(
                new StudioContentUpdateDraftRequest(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Guidance title",
                    "guidance.title",
                    RuntimeContentType.Tooltip,
                    "1.0.0"),
                cancellationToken);
            var review = await client.RequestReviewAsync(
                new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
                cancellationToken);
            var approve = await client.ApproveAsync(
                new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
                cancellationToken);
            var reject = await client.RejectAsync(
                new StudioWorkflowActionRequest(Guid.NewGuid(), Guid.NewGuid()),
                cancellationToken);
            var publish = await client.PublishAsync(
                new StudioPublishActionRequest(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "production",
                    DeliveryChannel.Published),
                cancellationToken);

            return new[]
            {
                create.Status,
                update.Status,
                review.Status,
                approve.Status,
                reject.Status,
                publish.Status
            }.All(status => status == StudioContentClientStatus.Unavailable);
        }
        catch
        {
            return false;
        }
    }

    private sealed class NoNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Preflight cannot send requests.");
    }
}
