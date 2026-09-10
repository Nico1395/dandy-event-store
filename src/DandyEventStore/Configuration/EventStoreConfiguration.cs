using System.Reflection;
using DandyEventStore.Configuration.Aggregates;
using DandyEventStore.Configuration.Events;
using DandyEventStore.Configuration.Outbox;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfiguration
{
    public required AggregatesConfiguration Aggregates { get; init; }
    public required EventsConfiguration Events { get; init; }
    public required OutboxConfiguration Outbox { get; init; }

    public required IReadOnlyDictionary<string, PluginConfiguration> Plugins { get; init; }
    public Assembly[]? Assemblies { get; init; }
    
    public Action<IServiceProvider, Exception>? OnOutboxPublishException { get; set; }
    public Action<IServiceProvider, Exception>? OnSubscriberException { get; set; }
}