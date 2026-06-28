namespace Adopta.Application.Runtime;

public sealed record RuntimeTargetingPlaceholder(
    string Mode,
    IReadOnlyCollection<string> Segments,
    IReadOnlyCollection<string> PageKeys);

