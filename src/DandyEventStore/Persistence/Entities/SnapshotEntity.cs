namespace DandyEventStore.Persistence.Entities;

public sealed class SnapshotEntity
{
    public required string StreamId { get; init; }
    public required string Payload { get; init; }
    public required string AggregateKey { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }
}