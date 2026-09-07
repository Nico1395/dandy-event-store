using System.Reflection;

namespace DandyEventStore.Aggregates.Configuration;

public sealed class AggregatesConfigurationBuilder
{
    private readonly AggregatesConfiguration _configuration = new();

    public AggregatesConfigurationBuilder AddAggregate<TAggregate>(Action<AggregateConfigurationBuilder<TAggregate>> builderAction)
        where TAggregate : class
    {
        var builder = new AggregateConfigurationBuilder<TAggregate>();
        builderAction(builder);

        var aggregateConfig = builder.Build();
        _configuration.AggregateConfigs[aggregateConfig.AggregateType] = aggregateConfig;
        
        return this;
    }

    public AggregatesConfigurationBuilder ScanInAssemblies(params Assembly[] assemblies)
    {
        _configuration.Assemblies = assemblies;
        return this;
    }

    internal AggregatesConfiguration Build()
    {
        return _configuration;
    }
}