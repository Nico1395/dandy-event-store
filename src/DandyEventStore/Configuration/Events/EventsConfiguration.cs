using System.Collections.Concurrent;
using System.Reflection;

namespace DandyEventStore.Configuration.Events;

public sealed class EventsConfiguration
{
    internal ConcurrentDictionary<Type, EventConfiguration> EventConfigsByType { get; } = new();
    internal ConcurrentDictionary<string, EventConfiguration> EventConfigsByKey { get; } = new();

    public IReadOnlyDictionary<Type, EventConfiguration> EventsByType => EventConfigsByType;
    public IReadOnlyDictionary<string, EventConfiguration> EventsByKey => EventConfigsByKey;

    public TimeSpan DefaultLifetime { get; set; } = TimeSpan.FromMinutes(15);

    internal EventConfiguration GetOrAddEventConfiguration(Type eventType)
    {
        return EventConfigsByType.GetOrAdd(eventType, type =>
        {
            var attribute = type.GetCustomAttribute<EventAttribute>();
            var configuration = new EventConfiguration
            {
                Key = attribute?.Key ?? eventType.Name,
                RuntimeType = eventType,
            };

            return EventConfigsByKey[configuration.Key] = configuration;
        });
    }
}