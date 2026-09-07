using System.Reflection;
using DandyEventStore.Aggregates.Configuration;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfiguration
{
    public required AggregatesConfiguration Aggregates { get; init; }
    public Assembly[]? Assemblies { get; init; }
}