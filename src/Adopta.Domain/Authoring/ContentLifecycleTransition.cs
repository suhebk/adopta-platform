namespace Adopta.Domain.Authoring;

public sealed record ContentLifecycleTransition(
    ContentLifecycleState From,
    ContentLifecycleState To)
{
    private static readonly IReadOnlySet<ContentLifecycleTransition> AllowedTransitions =
        new HashSet<ContentLifecycleTransition>
        {
            new(ContentLifecycleState.Draft, ContentLifecycleState.InReview),
            new(ContentLifecycleState.InReview, ContentLifecycleState.Draft),
            new(ContentLifecycleState.InReview, ContentLifecycleState.Approved),
            new(ContentLifecycleState.Approved, ContentLifecycleState.Published),
            new(ContentLifecycleState.Approved, ContentLifecycleState.Archived),
            new(ContentLifecycleState.Published, ContentLifecycleState.Archived),
            new(ContentLifecycleState.Draft, ContentLifecycleState.Archived)
        };

    public static bool IsAllowed(ContentLifecycleState from, ContentLifecycleState to)
    {
        return AllowedTransitions.Contains(new ContentLifecycleTransition(from, to));
    }
}
