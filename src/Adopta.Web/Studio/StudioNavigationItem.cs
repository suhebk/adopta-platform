namespace Adopta.Web.Studio;

public sealed record StudioNavigationItem(
    string RoutePath,
    string Label,
    string Group,
    string Description,
    string RequiredPermissionKey,
    string ShellStatus);
