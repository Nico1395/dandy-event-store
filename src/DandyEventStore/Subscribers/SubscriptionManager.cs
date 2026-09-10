using System.Diagnostics;
using DandyEventStore.Configuration;
using DandyEventStore.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Subscribers;

internal sealed class SubscriptionManager(
    EventStoreConfiguration eventStoreConfiguration,
    IServiceProvider serviceProvider) : ISubscriptionManager
{
    public async Task NotifySubscribersAsync(OutboxEnvelope outboxEnvelope, SubscriberMode[] modes, CancellationToken cancellationToken)
    {
        // We are looking for the event type in the subscribers-configuration. A subscriber will not be notified
        // if the subscriber type is not configured. A subscriber type is configured if it's manually added or has the 
        // SubscriberAttribute and is scanned.

        if (!eventStoreConfiguration.Subscribers.SubscribersByEventType.TryGetValue(outboxEnvelope.RuntimeType, out var subscriberConfigurations))
            return;

        var handleAsync = typeof(ISubscriber<>).MakeGenericType(outboxEnvelope.RuntimeType).GetMethod(nameof(ISubscriber<>.HandleAsync));
        if (handleAsync == null)
            throw new UnreachableException($"Subscribers of type '{handleAsync}' should have a method '{nameof(ISubscriber<>.HandleAsync)}'.");

        // Only resolve consumers that are configured with the right mode and that have not yet successfully consumed the event.
        // This way we can avoid resolving subscribers that should not receive the event or won't be invoked.

        var subscribers = subscriberConfigurations
            .Where(c => modes.Contains(c.Mode))
            .Where(c => outboxEnvelope.HasConsumed(c.Key))
            .Select(c => (c, serviceProvider.GetRequiredService(c.AbstractionType)));

        foreach (var (configuration, subscriber) in subscribers)
        {
            var context = new SubscriberContext { Envelope = outboxEnvelope, };

            try
            {
                if (handleAsync.Invoke(subscriber, [outboxEnvelope.Event, context, cancellationToken]) is not Task task)
                    throw new InvalidOperationException($"Subscriber of type '{handleAsync}' should return a Task.");

                await task;
                outboxEnvelope.Consume(configuration.Key, OutboxEventConsumerType.Subscriber);
            }
            catch (Exception exception)
            {
                outboxEnvelope.Fail(configuration.Key, OutboxEventConsumerType.Subscriber);
                eventStoreConfiguration.OnOutboxPublishException?.Invoke(serviceProvider, configuration, context, exception);
            }
        }
    }
}