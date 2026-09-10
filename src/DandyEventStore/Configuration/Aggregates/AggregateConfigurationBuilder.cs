using System.Diagnostics;

namespace DandyEventStore.Configuration.Aggregates;

public sealed class AggregateConfigurationBuilder<TAggregate>
    where TAggregate : class
{
    private readonly AggregateConfiguration _configuration = new()
    {
        Key = typeof(TAggregate).Name,
        RuntimeType = typeof(TAggregate),
    };

    public AggregateConfigurationBuilder<TAggregate> WithKey(string key)
    {
        _configuration.Key = key;
        return this;
    }

    public AggregateConfigurationBuilder<TAggregate> UseSnapshots(int interval)
    {
        _configuration.SnapshotInterval = interval;
        return this;
    }

    public AggregateConfigurationBuilder<TAggregate> UseFactory(Type factoryType)
    {
        _configuration.FactoryType = factoryType;
        return this;
    }

    public AggregateConfigurationBuilder<TAggregate> UseFactory(Func<TAggregate?, Envelope[], TAggregate> factoryFunc)
    {
        _configuration.FactoryFunc = (aggregate, envelopes) =>
        {
            return aggregate is not TAggregate casted
                ? throw new UnreachableException()
                : factoryFunc(casted, envelopes);
        };

        return this;
    }

    internal AggregateConfiguration Build()
    {
        return _configuration;
    }
}