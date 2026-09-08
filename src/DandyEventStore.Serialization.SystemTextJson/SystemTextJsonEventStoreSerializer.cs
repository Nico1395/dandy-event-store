using System.Text.Json;

namespace DandyEventStore.Serialization.SystemTextJson;

internal sealed class SystemTextJsonEventStoreSerializer(SystemTextJsonPluginConfiguration configuration) : IEventStoreSerializer
{
    public string Serialize(object eventItem, Type runtimeType)
    {
        return JsonSerializer.Serialize(eventItem, runtimeType, configuration.Options);
    }

    public object? Deserialize(string eventItem, Type eventType)
    {
        return JsonSerializer.Deserialize(eventItem, eventType, configuration.Options);
    }
}