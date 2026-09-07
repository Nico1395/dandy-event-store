namespace DandyEventStore.Persistence;

public sealed class RawEnvelope
{
    public required string StreamId { get; init; }
    public required string Payload { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
}