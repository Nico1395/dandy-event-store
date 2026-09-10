namespace DandyEventStore.Serialization;

public interface ISerializer
{
    string Serialize(object item, Type runtimeType);
    object? Deserialize(string payload, Type eventType);
}