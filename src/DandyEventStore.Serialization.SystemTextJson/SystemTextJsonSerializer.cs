using System.Text.Json;

namespace DandyEventStore.Serialization.SystemTextJson;

internal sealed class SystemTextJsonSerializer(SystemTextJsonConfiguration configuration) : ISerializer
{
    public string Serialize(object item, Type runtimeType)
    {
        return JsonSerializer.Serialize(item, runtimeType, configuration.Options);
    }

    public object? Deserialize(string payload, Type eventType)
    {
        return JsonSerializer.Deserialize(payload, eventType, configuration.Options);
    }
}