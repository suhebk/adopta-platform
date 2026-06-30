namespace Adopta.Web.Studio;

public enum StudioReadApiPreflightStatus
{
    Disabled = 0,
    Ready = 1,
    Invalid = 2
}

public enum StudioReadApiPreflightCheckStatus
{
    Passed = 0,
    Failed = 1,
    NotApplicable = 2
}

public static class StudioReadApiPreflightCheckCodes
{
    public const string StudioApiDisabledByDefault = nameof(StudioApiDisabledByDefault);
    public const string StudioApiBaseAddressConfigured = nameof(StudioApiBaseAddressConfigured);
    public const string StudioWebAuthenticationConfigured = nameof(StudioWebAuthenticationConfigured);
    public const string StudioApiTokenAcquisitionConfigured = nameof(StudioApiTokenAcquisitionConfigured);
    public const string RequestBoundaryHandlerRegistered = nameof(RequestBoundaryHandlerRegistered);
    public const string TenantHeaderNotClientSupplied = nameof(TenantHeaderNotClientSupplied);
    public const string TestHeadersNotProductionShortcuts = nameof(TestHeadersNotProductionShortcuts);
    public const string ReadClientIsReadOnly = nameof(ReadClientIsReadOnly);
    public const string LocalFallbackAvailable = nameof(LocalFallbackAvailable);
    public const string ActivationFailsClosed = nameof(ActivationFailsClosed);
}

public sealed record StudioReadApiPreflightCheck(
    string Code,
    StudioReadApiPreflightCheckStatus Status,
    string Message);

public sealed record StudioReadApiPreflightResult(
    StudioReadApiPreflightStatus Status,
    IReadOnlyCollection<StudioReadApiPreflightCheck> Checks)
{
    public bool IsReady => Status == StudioReadApiPreflightStatus.Ready;

    public bool HasFailures =>
        Checks.Any(check => check.Status == StudioReadApiPreflightCheckStatus.Failed);
}
