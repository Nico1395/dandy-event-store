namespace DandyEventStore.Projections;

public sealed class ProjectionContext
{
    public required IReadOnlyEventStore EventStore { get; init; }
    public required Envelope Envelope { get; init; }
    // public required int MaxRetries { get; init; }
    // public required int RetryCount { get; init; }
    // public required TimeSpan RetryBackoff { get; init; }
    public required ProjectionMode Mode { get; init; }
    public required int ProjectionIndex { get; init; }
    public required int ProjectionCount { get; init; }
}