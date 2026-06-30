namespace Adopta.Web.Studio;

public sealed class StudioApiTokenAcquisitionOptions
{
    public const string SectionName = "StudioApi:TokenAcquisition";

    public bool Enabled { get; set; }

    public string[] Scopes { get; set; } = [];

    public bool HasConfiguredScopes =>
        Scopes.Length > 0
        && Scopes.All(IsSafeScope);

    public static bool IsSafeScope(string? scope) =>
        !string.IsNullOrWhiteSpace(scope)
        && scope.IndexOfAny(['\r', '\n']) < 0
        && !scope.Contains(' ', StringComparison.Ordinal);
}
