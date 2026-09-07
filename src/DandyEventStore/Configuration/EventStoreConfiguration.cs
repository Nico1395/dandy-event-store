using System.Reflection;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Persistence.Configuration;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfiguration
{
    public required AggregatesConfiguration Aggregates { get; init; }
    public required PersistenceConfiguration Persistence { get; init; }
    public Assembly[]? Assemblies { get; init; }
}