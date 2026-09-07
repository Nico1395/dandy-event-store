namespace DandyEventStore;

public sealed class Snapshot
{
    private Type? _aggregateType;

    public required string StreamId { get; init; }
    public required object Aggregate { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }

    public Type AggregateType => _aggregateType ??= Aggregate.GetType();
}