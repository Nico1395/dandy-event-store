using System.Diagnostics.CodeAnalysis;

namespace DandyEventStore.Aggregates.Configuration;

public sealed class AggregateConfiguration
{
    public string Key { get; internal set; } = string.Empty;
    public required Type RuntimeType { get; init; }
    public Type? FactoryType { get; set; }
    public Func<object?, Envelope[], object>? FactoryFunc { get; internal set; }
    public int SnapshotInterval { get; internal set; } = -1;

    public bool UseSnapshots()
    {
        return SnapshotInterval > 0;
    }

    public bool ShouldCreateSnapshot(long originalStreamVersion, long streamVersion)
    {
        var diff = streamVersion - originalStreamVersion;
        return UseSnapshots() && diff % SnapshotInterval == 0;
    }
}