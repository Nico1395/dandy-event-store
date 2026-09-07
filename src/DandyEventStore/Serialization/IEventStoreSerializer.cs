namespace DandyEventStore.Serialization;

public interface IEventStoreSerializer
{
    string Serialize(object eventItem);
    object Deserialize(string eventItem, Type eventType);
}