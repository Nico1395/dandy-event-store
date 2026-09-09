namespace DandyEventStore.Subscribers;

public interface ISubscriberManager
{
    Task NotifySubscribersAsync(IReadOnlyEventStore? eventStore, Envelope[] envelopes, SubscriberMode mode, CancellationToken cancellationToken);
}