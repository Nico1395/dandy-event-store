using Newtonsoft.Json;

namespace DandyEventStore.Serialization.NewtonsoftJson;

internal sealed class NewtonsoftJsonEventStoreSerializer(NewtonsoftJsonConfiguration configuration) : IEventStoreSerializer
{
    public string Serialize(object eventItem, Type runtimeType)
    {
        return JsonConvert.SerializeObject(eventItem, runtimeType, configuration.Settings);
    }

    public object? Deserialize(string eventItem, Type eventType)
    {
        return JsonConvert.DeserializeObject(eventItem, eventType, configuration.Settings);
    }
}