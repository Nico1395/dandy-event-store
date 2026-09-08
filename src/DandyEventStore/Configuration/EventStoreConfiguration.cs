using System.Reflection;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Events;
using DandyEventStore.Events.Configurations;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfiguration
{
    public required AggregatesConfiguration Aggregates { get; init; }
    public required EventsConfiguration Events { get; init; }
    public required IReadOnlyDictionary<string, PluginConfiguration> Plugins { get; init; }
    public Assembly[]? Assemblies { get; init; }
}