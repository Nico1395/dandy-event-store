using System.Reflection;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Persistence.Configuration;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfigurationBuilder
{
    public AggregatesConfigurationBuilder Aggregates { get; } = new();
    public PersistenceConfigurationBuilder Persistence { get; } = new();

    public Assembly[]? Assemblies { get; set; }

    internal EventStoreConfiguration Build()
    {
        return new EventStoreConfiguration
        {
            Aggregates = Aggregates.Build(),
            Persistence = Persistence.Build(),
            Assemblies = Assemblies,
        };
    }
}