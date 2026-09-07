namespace DandyEventStore.Projections;

public static class ProjectorExtensions
{
    public static Task ProjectAsync(this IProjector projector, Envelope[] envelopes, ProjectionMode mode, CancellationToken cancellationToken)
    {
        return projector.ProjectAsync(eventStore: null, envelopes, mode, cancellationToken);
    }
}