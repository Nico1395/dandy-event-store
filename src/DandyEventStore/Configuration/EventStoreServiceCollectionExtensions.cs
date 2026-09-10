using System.Diagnostics;
using System.Reflection;
using DandyEventStore.Aggregates;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Events;
using DandyEventStore.Events.Configurations;
using DandyEventStore.Outbox;
using DandyEventStore.Subscribers;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Configuration;

public static class EventStoreServiceCollectionExtensions
{
    private static readonly IReadOnlyList<Type> _serviceTypes =
    [
        typeof(ISubscriber<>),
        typeof(ISubscriberExceptionHandler<>),
        typeof(IAggregateFactory<>),
    ];

    public static IServiceCollection AddDandyEventStore(this IServiceCollection services, Action<EventStoreConfigurationBuilder> builderAction)
    {
        var builder = new EventStoreConfigurationBuilder();
        builderAction(builder);
        var configuration = builder.Build();

        services.AddSingleton(configuration);
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<ISubscriberManager, SubscriberManager>();
        services.AddSingleton<IEnvelopeFactory, EnvelopeFactory>();

        if (configuration.Assemblies != null)
        {
            AddAggregatesFromAssemblies(configuration.Assemblies, configuration.Aggregates);
            AddEventsFromAssemblies(configuration.Assemblies, configuration.Events);
            AddServicesFromAssemblies(services, configuration.Assemblies);
        }

        foreach (var aggregateConfiguration in configuration.Aggregates.AggregatesByType.Values)
        {
            if (aggregateConfiguration.FactoryType == null)
                continue;

            services.AddTransient(
                typeof(IAggregateFactory<>).MakeGenericType(aggregateConfiguration.RuntimeType),
                aggregateConfiguration.FactoryType);
        }

        AddAggregateFactories(services, configuration.Aggregates);
        AddPlugins(services, configuration.Plugins);

        return services;
    }

    private static void AddAggregatesFromAssemblies(Assembly[] assemblies, AggregatesConfiguration configuration)
    {
        var aggregateTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.GetCustomAttribute<AggregateAttribute>() != null);

        foreach (var aggregateType in aggregateTypes)
        {
            var attribute = aggregateType.GetCustomAttribute<AggregateAttribute>();
            if (attribute == null)
                throw new UnreachableException();

            var aggregateConfiguration = configuration.CreateAggregateConfiguration(aggregateType, attribute);

            configuration.AggregateConfigsByType[aggregateType] = aggregateConfiguration;
            configuration.AggregateConfigsByKey[aggregateConfiguration.Key] = aggregateConfiguration;
        }
    }

    private static void AddEventsFromAssemblies(Assembly[] assemblies, EventsConfiguration configuration)
    {
        var eventTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.GetCustomAttribute<EventAttribute>() != null);
        
        foreach (var eventType in eventTypes)
        {
            var attribute = eventType.GetCustomAttribute<EventAttribute>();
            if (attribute == null)
                throw new UnreachableException();

            var eventConfiguration = configuration.CreateEventConfiguration(eventType, attribute);

            configuration.EventConfigsByType[eventType] = eventConfiguration;
            configuration.EventConfigsByKey[eventConfiguration.Key] = eventConfiguration;
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

    private static void AddAggregateFactories(IServiceCollection services, AggregatesConfiguration configuration)
    {
        foreach (var aggregateConfiguration in configuration.AggregatesByType.Values)
        {
            if (aggregateConfiguration.FactoryType == null)
                continue;

            services.AddTransient(
                typeof(IAggregateFactory<>).MakeGenericType(aggregateConfiguration.RuntimeType),
                aggregateConfiguration.FactoryType);
        }
    }

    private static void AddPlugins(IServiceCollection services, IReadOnlyDictionary<string, PluginConfiguration> plugins)
    {
        foreach (var (_, configuration) in plugins)
            configuration.ConfigureServices(services);
    }
}