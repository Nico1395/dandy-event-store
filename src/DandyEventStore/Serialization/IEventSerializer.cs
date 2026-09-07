namespace DandyEventStore.Serialization;

public interface IEventSerializer
{
    string Serialize(object eventItem);
    object Deserialize(string eventItem, Type eventType);
}