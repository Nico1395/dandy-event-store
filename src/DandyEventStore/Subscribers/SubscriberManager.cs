using System.Diagnostics;
using System.Reflection;
using DandyEventStore.Configuration;
using DandyEventStore.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Subscribers;

internal sealed class SubscriberManager(
    EventStoreConfiguration eventStoreConfiguration,
    IServiceProvider serviceProvider) : ISubscriberManager
{
    public async Task NotifySyncSubscribersAsync(OutboxEnvelope outboxEnvelope, CancellationToken cancellationToken)
    {
        var syncSubscriberType = typeof(ISubscriber<>).MakeGenericType(outboxEnvelope.RuntimeType);
        MethodInfo? handleAsync = null;

        var subscribers = serviceProvider.GetServices(syncSubscriberType);
        foreach (var subscriber in subscribers.Where(s => s != null))
        {
            var consumerKey = subscriber!.GetType().Name;

            try
            {
                handleAsync ??= syncSubscriberType.GetMethod(nameof(ISubscriber<>.HandleAsync));
                if (handleAsync == null)
                    throw new UnreachableException($"Subscribers of type '{syncSubscriberType}' should have a method '{nameof(ISubscriber<>.HandleAsync)}'.");

                var context = new SubscriberContext { Event = outboxEnvelope, };
                if (handleAsync.Invoke(subscriber, [outboxEnvelope, context, cancellationToken]) is not Task task)
                    throw new InvalidOperationException($"Subscriber of type '{syncSubscriberType}' should return a Task.");

                await task;

                outboxEnvelope.Consume(consumerKey, OutboxEventConsumerType.Subscriber);
            }
            catch (Exception ex)
            {
                outboxEnvelope.Fail(consumerKey, OutboxEventConsumerType.Subscriber);
                eventStoreConfiguration.OnOutboxPublishException?.Invoke(serviceProvider, ex);
            }
        }
    }
}