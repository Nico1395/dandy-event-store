using System.Diagnostics;
using DandyEventStore.Configuration;
using DandyEventStore.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Subscribers;

internal sealed class SubscriptionManager(
    EventStoreConfiguration eventStoreConfiguration,
    IServiceProvider serviceProvider) : ISubscriptionManager
{
    public async Task NotifySubscribersAsync(OutboxEnvelope outboxEnvelope, SubscriberMode mode, CancellationToken cancellationToken)
    {
        // We are looking for the event type in the subscribers-configuration. A subscriber will not be notified
        // if the subscriber type is not configured. A subscriber type is configured if it's manually added or has the 
        // SubscriberAttribute and is scanned.

        if (!eventStoreConfiguration.Subscribers.SubscribersByEventType.TryGetValue(outboxEnvelope.RuntimeType, out var subscriberConfigurations))
            return;

        var handleAsync = typeof(ISubscriber<>).MakeGenericType(outboxEnvelope.RuntimeType).GetMethod(nameof(ISubscriber<>.HandleAsync));
        if (handleAsync == null)
            throw new UnreachableException($"Subscribers of type '{handleAsync}' should have a method '{nameof(ISubscriber<>.HandleAsync)}'.");

        var subscribers = subscriberConfigurations
            .Where(c => c.Mode == mode)
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