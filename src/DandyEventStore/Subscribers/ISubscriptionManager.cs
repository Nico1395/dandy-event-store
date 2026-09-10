using DandyEventStore.Outbox;

namespace DandyEventStore.Subscribers;

internal interface ISubscriptionManager
{
    Task NotifySubscribersAsync(IEventStore? eventStore, OutboxEnvelope outboxEnvelope, SubscriberMode[] modes, CancellationToken cancellationToken);
}