namespace Adopta.Application.Runtime;

public sealed record RuntimeExperienceMetadata(
    RuntimeRendererPlacement? Placement = null,
    IReadOnlyCollection<string>? DismissBehavior = null,
    RuntimeRendererTheme? Theme = null);
