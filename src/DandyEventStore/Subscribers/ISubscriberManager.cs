using DandyEventStore.Outbox;

namespace DandyEventStore.Subscribers;

public interface ISubscriberManager
{
    Task NotifySyncSubscribersAsync(OutboxEnvelope outboxEnvelope, CancellationToken cancellationToken);
}