namespace DandyEventStore;

public sealed class Envelope
{
    private Type? _eventType;

    public required string StreamId { get; init; }
    public required object Event { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventTypeKey { get; init; }

    public Type EventType => _eventType ??= Event.GetType();
}