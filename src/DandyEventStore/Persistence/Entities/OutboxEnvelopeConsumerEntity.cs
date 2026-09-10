using DandyEventStore.Outbox;

namespace DandyEventStore.Persistence.Entities;

public sealed class OutboxEnvelopeConsumerEntity
{
    public required string StreamId { get; init; }
    public required long Version { get; init; }
    public required string ConsumerKey { get; init; }
    public required OutboxEventConsumerType Type { get; init; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime? FailedAt { get; set; }
}