using System.Collections.Concurrent;
using System.Reflection;

namespace DandyEventStore.Aggregates.Configuration;

public sealed class AggregatesConfiguration
{
    internal ConcurrentDictionary<Type, AggregateConfiguration> AggregateConfigs { get; } = new();

    public IReadOnlyDictionary<Type, AggregateConfiguration> Aggregates => AggregateConfigs;
    public Assembly[]? Assemblies { get; internal set; }

    internal AggregateConfiguration GetOrAdd(Type aggregateType)
    {
        return AggregateConfigs.GetOrAdd(aggregateType, CreateAggregateConfiguration);
    }

    internal AggregateConfiguration CreateAggregateConfiguration(Type aggregateType)
    {
        // TODO: Grab a factory method
        // 1. Either a constructor accepting TAggregate? snapshot, Envelope[] envelopes
        // 2. Static factory method accepting TAggregate? snapshot, Envelope[] envelopes

    }
}