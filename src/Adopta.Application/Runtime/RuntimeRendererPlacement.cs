namespace Adopta.Application.Runtime;

public sealed record RuntimeRendererPlacement(
    string Preferred,
    IReadOnlyCollection<string>? Fallback = null);
