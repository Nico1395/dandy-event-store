using System.Diagnostics;
using System.Reflection;
using DandyEventStore.Aggregates;
using DandyEventStore.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Configuration;

public static class EventStoreServiceCollectionExtensions
{
    private static readonly IReadOnlyList<Type> _serviceTypes =
    [
        typeof(IProjection<>),
        typeof(IProjectionExceptionHandler<>),
        typeof(IAggregateFactory<>),
    ];

    public static IServiceCollection AddDandyEventStore(this IServiceCollection services, Action<EventStoreConfigurationBuilder> builderAction)
    {
        var builder = new EventStoreConfigurationBuilder();
        builderAction(builder);
        var configuration = builder.Build();

        services.AddSingleton(configuration);
        services.AddSingleton<IEventStore, EventStore>();
        services.AddSingleton<IProjector, Projector>();

        if (configuration.Assemblies != null)
        {
            AddAggregatesFromAssemblies(configuration, configuration.Assemblies);
            AddServicesFromAssemblies(services, configuration.Assemblies);
        }

        foreach (var aggregateConfiguration in configuration.Aggregates.Aggregates.Values.Where(a => a.FactoryType != null))
        {
            if (aggregateConfiguration.FactoryType == null)
                throw new UnreachableException();

            services.AddTransient(
                typeof(IAggregateFactory<>).MakeGenericType(aggregateConfiguration.AggregateType),
                aggregateConfiguration.FactoryType);
        }

        return services;
    }

    private static void AddAggregatesFromAssemblies(EventStoreConfiguration configuration, Assembly[] assemblies)
    {
        var aggregateTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.GetCustomAttribute<AggregateAttribute>() != null);

        foreach (var aggregateType in aggregateTypes)
        {
            var attribute = aggregateType.GetCustomAttribute<AggregateAttribute>();
            if (attribute == null)
                throw new UnreachableException();

            var aggregateConfiguration = configuration.Aggregates.CreateAggregateConfiguration(aggregateType, attribute);
            configuration.Aggregates.AggregateConfigs[aggregateType] = aggregateConfiguration;
        }
    }

    private static void AddServicesFromAssemblies(IServiceCollection services, IReadOnlyList<Assembly> assemblies)
    {
        var handlerTypes = assemblies.SelectMany(a => a.DefinedTypes).Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });
        foreach (var implementationType in handlerTypes)
        {
            var interfaces = implementationType.ImplementedInterfaces;
            foreach (var @interface in interfaces)
            {
                if (!@interface.IsGenericType)
                    continue;

                var genericDefinition = @interface.GetGenericTypeDefinition();
                if (_serviceTypes.Contains(genericDefinition))
                    services.AddTransient(@interface, implementationType);
            }
        }
    }
}