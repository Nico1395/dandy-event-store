using System.Diagnostics;
using System.Reflection;
using DandyEventStore.Configuration.Aggregates;
using DandyEventStore.Configuration.Events;
using DandyEventStore.Configuration.Subscribers;
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
            AddSubscribersFromAssemblies(configuration.Assemblies, configuration.Subscribers);
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
            configuration.GetOrAddAggregateConfiguration(aggregateType);
    }

    private static void AddEventsFromAssemblies(Assembly[] assemblies, EventsConfiguration configuration)
    {
        var eventTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.GetCustomAttribute<EventAttribute>() != null);
        
        foreach (var eventType in eventTypes)
            configuration.GetOrAddEventConfiguration(eventType);
    }

    private static void AddSubscribersFromAssemblies(Assembly[] assemblies, SubscribersConfiguration configuration)
    {
        var subscriberTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.GetCustomAttribute<SubscriberAttribute>() != null);
        
        foreach (var subscriberType in subscriberTypes)
            configuration.GetOrAddSubscriberConfiguration(subscriberType);
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