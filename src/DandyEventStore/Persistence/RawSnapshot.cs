namespace DandyEventStore.Persistence;

public sealed class RawSnapshot
{
    public required string StreamId { get; init; }
    public required string Payload { get; init; }
    public required string AggregateType { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }
}