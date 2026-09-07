using System.Reflection;
using DandyEventStore.Aggregates.Configuration;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfigurationBuilder
{
    public AggregatesConfigurationBuilder Aggregates { get; } = new();
    public Assembly[]? Assemblies { get; set; }

    internal EventStoreConfiguration Build()
    {
        return new EventStoreConfiguration
        {
            Aggregates = Aggregates.Build(),
            Assemblies = Assemblies,
        };
    }
}