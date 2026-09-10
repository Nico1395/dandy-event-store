namespace DandyEventStore.Outbox;

public sealed class OutboxEnvelope : IReadOnlyOutboxEnvelope
{
    public required string StreamId { get; init; }
    public required object Event { get; init; }
    public required long Version { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventKey { get; init; }
    public required Type RuntimeType { get; init; }
    public List<OutboxEnvelopeConsumer> Consumers { get; init; } = [];

    public static OutboxEnvelope Create(Envelope envelope)
    {
        return new OutboxEnvelope
        {
            StreamId = envelope.StreamId,
            Event = envelope.Event,
            Version = envelope.Version,
            Timestamp = envelope.Timestamp,
            EventKey = envelope.EventKey,
            RuntimeType = envelope.RuntimeType,
        };
    }

    public bool IsExpired(TimeSpan eventLifetime)
    {
        return DateTime.UtcNow >= Timestamp + eventLifetime;
    }

    public void Consume(string consumerKey, OutboxEventConsumerType type)
    {
        var consumer = Consumers.SingleOrDefault(c => c.ConsumerKey == consumerKey && !c.ConsumedAt.HasValue);
        if (consumer == null)
        {
            Consumers.Add(consumer = new OutboxEnvelopeConsumer
            {
                StreamId = StreamId,
                Version = Version,
                ConsumerKey = consumerKey,
                Type = type,
            });
        }

        consumer.Consume();
    }

    public void Fail(string consumerKey, OutboxEventConsumerType type)
    {
        var consumer = Consumers.SingleOrDefault(c => c.ConsumerKey == consumerKey && !c.ConsumedAt.HasValue);
        if (consumer == null)
        {
            Consumers.Add(consumer = new OutboxEnvelopeConsumer
            {
                StreamId = StreamId,
                Version = Version,
                ConsumerKey = consumerKey,
                Type = type,
            });
        }

        consumer.Fail();
    }
}