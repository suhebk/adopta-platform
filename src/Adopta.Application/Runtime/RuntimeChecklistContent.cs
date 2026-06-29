namespace Adopta.Application.Runtime;

public sealed record RuntimeChecklistContent(
    IReadOnlyCollection<RuntimeChecklistStep> Steps);

public sealed record RuntimeChecklistStep(
    string Id,
    string Title,
    string? Body = null,
    RuntimeAnchorDescriptor? Anchor = null,
    RuntimeExperienceMetadata? Experience = null);
