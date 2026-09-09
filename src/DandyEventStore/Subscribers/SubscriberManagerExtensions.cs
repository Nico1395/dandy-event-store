namespace DandyEventStore.Subscribers;

public static class SubscriberManagerExtensions
{
    public static Task NotifySubscribersAsync(this ISubscriberManager subscriberManager, Envelope[] envelopes, SubscriberMode mode, CancellationToken cancellationToken)
    {
        return subscriberManager.NotifySubscribersAsync(eventStore: null, envelopes, mode, cancellationToken);
    }
}