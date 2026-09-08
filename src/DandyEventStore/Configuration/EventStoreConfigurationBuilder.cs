using System.Reflection;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Events;
using DandyEventStore.Events.Configurations;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfigurationBuilder
{
    private Dictionary<string, PluginConfiguration> Plugins { get; set; } = [];

    public AggregatesConfigurationBuilder Aggregates { get; } = new();
    public EventsConfigurationBuilder Events { get; } = new();
    public Assembly[]? Assemblies { get; set; }

    public EventStoreConfigurationBuilder UsePlugin(PluginConfiguration plugin)
    {
        Plugins[plugin.Slot] = plugin;
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
            Plugins = Plugins,
            Assemblies = Assemblies,
        };
    }
}