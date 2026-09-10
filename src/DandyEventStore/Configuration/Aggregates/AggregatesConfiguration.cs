using System.Collections.Concurrent;
using System.Reflection;

namespace DandyEventStore.Configuration.Aggregates;

public sealed class AggregatesConfiguration
{
    internal ConcurrentDictionary<Type, AggregateConfiguration> AggregateConfigsByType { get; } = new();
    internal ConcurrentDictionary<string, AggregateConfiguration> AggregateConfigsByKey { get; } = new();

    public IReadOnlyDictionary<Type, AggregateConfiguration> AggregatesByType => AggregateConfigsByType;
    public IReadOnlyDictionary<string, AggregateConfiguration> AggregatesByKey => AggregateConfigsByKey;

    internal AggregateConfiguration GetOrAddAggregateConfiguration(Type aggregateType)
    {
        return AggregateConfigsByType.GetOrAdd(aggregateType, type =>
        {
            var configuration = CreateAggregateConfiguration(type, aggregateType.GetCustomAttribute<AggregateAttribute>());
            return AggregateConfigsByKey[configuration.Key] = configuration;
        });
    }

    internal AggregateConfiguration CreateAggregateConfiguration(Type aggregateType, AggregateAttribute? attribute)
    {
        var configuration = new AggregateConfiguration
        {
            Key = attribute?.Key ?? aggregateType.Name,
            RuntimeType = aggregateType,
            SnapshotInterval = attribute?.SnapshotInterval ?? -1,
        };

        var factoryMethod = aggregateType
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(method => method.IsDefined(typeof(AggregateFactoryAttribute), inherit: false))
            .Where(method => method.ReturnType == aggregateType)
            .SingleOrDefault(method => HasFactoryParameters(method.GetParameters(), aggregateType));
        if (factoryMethod != null)
        {
            configuration.FactoryFunc = (snapshot, envelopes) => factoryMethod.Invoke(null, [snapshot, envelopes])!;
            return configuration;
        }

        var factoryConstructor = aggregateType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(constructor => constructor.IsDefined(typeof(AggregateFactoryAttribute), inherit: false))
            .SingleOrDefault(constructor => HasFactoryParameters(constructor.GetParameters(), aggregateType));
        if (factoryConstructor != null)
            configuration.FactoryFunc = (snapshot, envelopes) => factoryConstructor.Invoke([snapshot, envelopes])!;

        return configuration;
    }

    private static bool HasFactoryParameters(ParameterInfo[] parameters, Type aggregateType)
    {
        return parameters.Length == 2 &&
            parameters[0].ParameterType == aggregateType &&
            parameters[1].ParameterType == typeof(Envelope[]);
    }
}
