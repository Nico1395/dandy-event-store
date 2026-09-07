namespace DandyEventStore.Aggregates;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AggregateAttribute : Attribute
{
    public int? SnapshotInterval { get; init; }
}