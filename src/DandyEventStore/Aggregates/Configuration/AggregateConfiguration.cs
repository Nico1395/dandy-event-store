namespace DandyEventStore.Aggregates.Configuration;

public sealed class AggregateConfiguration
{
    public required Type AggregateType { get; init; }
    public Type? FactoryType { get; set; }
    public Func<object?, Envelope[], object>? FactoryFunc { get; internal set; }
    public int? SnapshotInterval { get; internal set; }

    public bool UseSnapshots()
    {
        return SnapshotInterval.HasValue;
    }
}