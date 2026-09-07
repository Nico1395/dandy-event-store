namespace DandyEventStore.Projections;

public interface IProjector
{
    Task ProjectAsync(IReadOnlyEventStore? eventStore, Envelope[] envelopes, ProjectionMode mode, CancellationToken cancellationToken);
}