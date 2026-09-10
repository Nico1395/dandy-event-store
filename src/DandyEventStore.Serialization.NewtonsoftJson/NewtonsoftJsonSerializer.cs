using Newtonsoft.Json;

namespace DandyEventStore.Serialization.NewtonsoftJson;

internal sealed class NewtonsoftJsonSerializer(NewtonsoftJsonConfiguration configuration) : ISerializer
{
    public string Serialize(object item, Type runtimeType)
    {
        return JsonConvert.SerializeObject(item, runtimeType, configuration.Settings);
    }

    public object? Deserialize(string payload, Type eventType)
    {
        return JsonConvert.DeserializeObject(payload, eventType, configuration.Settings);
    }
}