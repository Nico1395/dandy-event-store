using System.Reflection;
using DandyEventStore.Configuration.Aggregates;
using DandyEventStore.Configuration.Events;
using DandyEventStore.Configuration.Outbox;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfigurationBuilder
{
    private readonly Dictionary<string, PluginConfiguration> _plugins = [];

    public AggregatesConfigurationBuilder Aggregates { get; } = new();
    public EventsConfigurationBuilder Events { get; } = new();
    public OutboxConfigurationBuilder Outbox { get; } = new();

    public Assembly[]? Assemblies { get; set; }

    public Action<IServiceProvider, Exception>? OnOutboxPublishException { get; set; }
    public Action<IServiceProvider, Exception>? OnSubscriberException { get; set; }

    public EventStoreConfigurationBuilder UsePlugin(PluginConfiguration plugin)
    {
        _plugins[plugin.Slot] = plugin;
        return this;
    }

    public EventStoreConfigurationBuilder ScanInAssemblies(params Assembly[] assemblies)
    {
        Assemblies = assemblies;
        return this;
    }

    internal EventStoreConfiguration Build()
    {
        return new EventStoreConfiguration
        {
            Aggregates = Aggregates.Build(),
            Events = Events.Build(),
            Outbox = Outbox.Build(),
            Plugins = _plugins,
            Assemblies = Assemblies,
            OnOutboxPublishException = OnOutboxPublishException,
            OnSubscriberException = OnSubscriberException,
        };
    }
}