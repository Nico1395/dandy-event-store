namespace DandyEventStore.Serialization;

public interface IEventStoreSerializer
{
    string Serialize(object eventItem, Type runtimeType);
    object? Deserialize(string eventItem, Type eventType);
}