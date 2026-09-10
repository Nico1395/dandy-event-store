namespace DandyEventStore.Aggregates;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AggregateAttribute : Attribute
{
    public string? Key { get; init; }
    public int SnapshotInterval { get; init; } = -1;
}