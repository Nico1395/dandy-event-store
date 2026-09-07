using DandyEventStore.Aggregates.Configuration;

namespace DandyEventStore.Configuration;

public sealed class EventStoreConfigurationBuilder
{
    public AggregatesConfigurationBuilder Aggregates { get; } = new();

    internal EventStoreConfiguration Build()
    {
        return new EventStoreConfiguration
        {
            Aggregates = Aggregates.Build(),
        };
    }
}