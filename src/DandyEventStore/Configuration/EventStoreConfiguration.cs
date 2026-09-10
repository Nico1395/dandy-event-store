using System.Reflection;
using DandyEventStore.Configuration.Aggregates;
using DandyEventStore.Configuration.Events;
using DandyEventStore.Configuration.Outbox;
using DandyEventStore.Configuration.Subscribers;
using DandyEventStore.Subscribers;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfiguration
{
    internal EventStoreConfiguration()
    {
    }

    public required AggregatesConfiguration Aggregates { get; init; }
    public required EventsConfiguration Events { get; init; }
    public required SubscribersConfiguration Subscribers { get; init; }
    public required OutboxConfiguration Outbox { get; init; }

    public Assembly[]? Assemblies { get; init; }

    public Action<IServiceProvider, SubscriberConfiguration, SubscriberContext, Exception>? OnOutboxPublishException { get; set; }
    public Action<IServiceProvider, Exception>? OnSubscriberException { get; set; }

    internal IReadOnlyDictionary<string, PluginConfiguration> Plugins { get; init; } = new Dictionary<string, PluginConfiguration>();
}