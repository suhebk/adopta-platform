namespace Adopta.Application.Runtime;

public sealed record RuntimeContentItem(
    string Id,
    RuntimeContentType Type,
    string Version,
    string Title,
    string? Body,
    RuntimeAnchorDescriptor? Anchor,
    RuntimeTargetingPlaceholder? Targeting);

