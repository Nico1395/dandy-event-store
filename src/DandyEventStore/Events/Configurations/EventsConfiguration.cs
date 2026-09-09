using System.Collections.Concurrent;
using System.Reflection;

namespace DandyEventStore.Events.Configurations;

public sealed class EventsConfiguration
{
    internal ConcurrentDictionary<Type, EventConfiguration> EventConfigsByType { get; } = new();
    internal ConcurrentDictionary<string, EventConfiguration> EventConfigsByKey { get; } = new();

    public IReadOnlyDictionary<Type, EventConfiguration> EventsByType => EventConfigsByType;
    public IReadOnlyDictionary<string, EventConfiguration> EventsByKey => EventConfigsByKey;

    public TimeSpan DefaultLifetime { get; set; } = TimeSpan.FromMinutes(15);

    internal EventConfiguration GetOrAddEventConfig(Type eventType)
    {
        return EventConfigsByType.GetOrAdd(eventType, type =>
        {
            var configuration = CreateEventConfiguration(type, type.GetCustomAttribute<EventAttribute>());

            EventConfigsByKey[configuration.Key] = configuration;

            return configuration;
        });
    }

    internal EventConfiguration CreateEventConfiguration(Type eventType, EventAttribute? attribute)
    {
        var configuration = new EventConfiguration
        {
            Key = attribute?.Key ?? eventType.Name,
            RuntimeType = eventType,
        };

        return configuration;
    }
}