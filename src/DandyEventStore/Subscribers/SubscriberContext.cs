using DandyEventStore.Configuration.Subscribers;
using DandyEventStore.Outbox;

namespace DandyEventStore.Subscribers;

public sealed class SubscriberContext
{
    public required IEventStore EventStore { get; init; }
    public required IReadOnlyOutboxEnvelope Envelope { get; init; }
    public required SubscriberConfiguration Configuration { get; init; }

    // public required int MaxRetries { get; init; }
    // public required int RetryCount { get; init; }
    // public required TimeSpan RetryBackoff { get; init; }
}