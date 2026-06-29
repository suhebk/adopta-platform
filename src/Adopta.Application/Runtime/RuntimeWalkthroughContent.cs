namespace Adopta.Application.Runtime;

public sealed record RuntimeWalkthroughContent(
    IReadOnlyCollection<RuntimeWalkthroughStep> Steps);

public sealed record RuntimeWalkthroughStep(
    string Id,
    string Title,
    string? Body = null,
    RuntimeAnchorDescriptor? Anchor = null,
    RuntimeExperienceMetadata? Experience = null);
