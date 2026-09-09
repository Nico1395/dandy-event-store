namespace DandyEventStore.Outbox;

public interface IReadOnlyOutboxEvent
{
    string StreamId { get; }
    object Event { get; }
    long Version { get; }
    DateTime Timestamp { get; }
    string EventKey { get; }
    Type RuntimeType { get; }
}