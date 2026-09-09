namespace DandyEventStore.Subscribers;

public sealed class SubscriberContext
{
    public required IReadOnlyEventStore EventStore { get; init; }
    public required Envelope Envelope { get; init; }

    // public required int MaxRetries { get; init; }
    // public required int RetryCount { get; init; }
    // public required TimeSpan RetryBackoff { get; init; }
}