using System.Collections.Concurrent;

namespace DandyEventStore;

public static class EventTypeProvider
{
    private static readonly ConcurrentDictionary<string, Type> _eventTypes = [];

    public static Type Get(string eventType)
    {
        return _eventTypes.GetOrAdd(eventType, type =>
        {
            return Type.GetType(type) ??
                   throw new InvalidOperationException($"Could not find event type '{type}'.");
        });
    }
}