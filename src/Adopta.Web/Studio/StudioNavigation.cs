using Adopta.Application.Identity;

namespace Adopta.Web.Studio;

public static class StudioNavigation
{
    public const string FoundationShellStatus = "Foundation shell";

    public static readonly IReadOnlyCollection<StudioNavigationItem> Items =
    [
        new(
            "/studio",
            "Studio Overview",
            "Studio",
            "Entry point for Adoption Studio authoring and governance foundations.",
            AdoptaPermissionKeys.AuthoringRead,
            FoundationShellStatus),
        new(
            "/studio/content",
            "Authored Content",
            "Authoring",
            "Foundation route for authored guidance inventory.",
            AdoptaPermissionKeys.AuthoringRead,
            FoundationShellStatus),
        new(
            "/studio/review",
            "Review Queue",
            "Governance",
            "Foundation route for content review and approval queues.",
            AdoptaPermissionKeys.AuthoringReview,
            FoundationShellStatus),
        new(
            "/studio/publishing",
            "Publishing",
            "Delivery",
            "Foundation route for publishing readiness and bundle mapping.",
            AdoptaPermissionKeys.AuthoringPublish,
            FoundationShellStatus),
        new(
            "/studio/governance",
            "Governance & Audit",
            "Governance",
            "Foundation route for audit and governance visibility.",
            AdoptaPermissionKeys.AuditRead,
            FoundationShellStatus)
    ];
}
