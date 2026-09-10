namespace DandyEventStore.Configuration.Aggregates;

public sealed class AggregatesConfigurationBuilder
{
    private readonly AggregatesConfiguration _configuration = new();

    public AggregatesConfigurationBuilder AddAggregate<TAggregate>(Action<AggregateConfigurationBuilder<TAggregate>> builderAction)
        where TAggregate : class
    {
        var builder = new AggregateConfigurationBuilder<TAggregate>();
        builderAction(builder);
        var configuration = builder.Build();

        _configuration.AggregateConfigsByType[configuration.RuntimeType] = configuration;
        _configuration.AggregateConfigsByKey[configuration.Key] = configuration;

        return this;
    }

    internal AggregatesConfiguration Build()
    {
        return _configuration;
    }
}