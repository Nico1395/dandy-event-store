using DandyEventStore.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Configuration;

public static class EventStoreServiceCollectionExtensions
{
    // TODO: Add projections

    public static IServiceCollection AddDandyEventStore(this IServiceCollection services, Action<EventStoreConfigurationBuilder> builderAction)
    {
        var builder = new EventStoreConfigurationBuilder();
        builderAction(builder);
        var configuration = builder.Build();

        services.AddSingleton(configuration);
        services.AddSingleton<IEventStore, EventStore>();
        services.AddSingleton<IProjector, Projector>();

        return services;
    }
}