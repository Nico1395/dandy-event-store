using DandyEventStore.Outbox;

namespace DandyEventStore.Subscribers;

internal interface ISubscriptionManager
{
    Task NotifySubscribersAsync(OutboxEnvelope outboxEnvelope, SubscriberMode[] modes, CancellationToken cancellationToken);
}