namespace DandyEventStore;

public sealed class Envelope
{
    public required string StreamId { get; init; }
    public required object Event { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required Type RuntimeType { get; init; }
}