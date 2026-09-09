namespace DandyEventStore.Serialization;

public interface ISerializer
{
    string Serialize(object eventItem, Type runtimeType);
    object? Deserialize(string eventItem, Type eventType);
}