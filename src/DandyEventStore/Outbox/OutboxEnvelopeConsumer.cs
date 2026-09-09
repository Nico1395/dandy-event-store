namespace DandyEventStore.Outbox;

public sealed class OutboxEnvelopeConsumer
{
    public required string StreamId { get; init; }
    public required long Version { get; init; }
    public required string ConsumerKey { get; init; }
    public required OutboxEventConsumerType Type { get; init; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime? FailedAt { get; set; }

    public void Consume()
    {
        ConsumedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        FailedAt = DateTime.UtcNow;
    }
}