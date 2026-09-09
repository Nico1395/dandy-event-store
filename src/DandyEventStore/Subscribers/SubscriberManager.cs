using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Subscribers;

internal sealed class SubscriberManager(IServiceProvider serviceProvider) : ISubscriberManager
{
    public async Task NotifySubscribersAsync(IReadOnlyEventStore? eventStore, Envelope[] envelopes, SubscriberMode mode, CancellationToken cancellationToken)
    {
        eventStore ??= serviceProvider.GetRequiredService<IReadOnlyEventStore>();
        var genericSubscriberInterface = mode == SubscriberMode.Async ? typeof(IAsyncSubscriber<>) : typeof(ISubscriber<>);

        foreach (var group in envelopes.GroupBy(e => e.RuntimeType))
        {
            var groupEnvelopes = group.ToArray();
            if (groupEnvelopes.Length == 0)
                continue;

            var subscriberType = genericSubscriberInterface.MakeGenericType(group.Key);
            var subscribers = serviceProvider.GetServices(subscriberType).Cast<object>().ToArray();

            await OrchestrateSubscribersAsync(eventStore, group.Key, groupEnvelopes, subscribers, cancellationToken);
        }
    }

    private async Task OrchestrateSubscribersAsync(IReadOnlyEventStore eventStore, Type eventType, Envelope[] envelopes, object[] subscribers, CancellationToken cancellationToken)
    {
        if (envelopes.Length == 0 || subscribers.Length == 0)
            return;

        var handleAsync = typeof(ISubscriber<>).GetMethod(nameof(ISubscriber<>.HandleAsync));
        if (handleAsync == null)
            throw new UnreachableException($"The subscriber abstraction was expected to have a method '{nameof(ISubscriber<>.HandleAsync)}'.");

        Type? exceptionHandlerType = null;
        MethodInfo? handleExceptionAsync = null;

        foreach (var envelope in envelopes)
        {
            foreach (var subscriber in subscribers)
            {
                exceptionHandlerType ??= typeof(ISubscriberExceptionHandler<>).MakeGenericType(eventType);
                handleExceptionAsync ??= exceptionHandlerType.GetMethod(nameof(ISubscriberExceptionHandler<>.HandleAsync));

                if (handleExceptionAsync == null)
                    throw new UnreachableException($"Subscriber exception handler {exceptionHandlerType.FullName} does not have a method '{nameof(ISubscriberExceptionHandler<>.HandleAsync)}' even though it implements {typeof(ISubscriberExceptionHandler<>)}.");

                await InvokeSubscriberAsync(
                    eventStore,
                    subscriber,
                    envelope,
                    handleAsync,
                    exceptionHandlerType,
                    handleExceptionAsync,
                    cancellationToken);
            }
        }
    }

    private async Task InvokeSubscriberAsync(IReadOnlyEventStore eventStore, object subscriber, Envelope envelope, MethodInfo handleAsync, Type exceptionHandlerType, MethodInfo handleExceptionAsync, CancellationToken cancellationToken)
    {
        var context = new SubscriberContext
        {
            EventStore = eventStore,
            Envelope = envelope,
        };

        try
        {
            var result = handleAsync.Invoke(subscriber, [envelope.Event, context, cancellationToken]);
            if (result is not Task task)
                throw new UnreachableException($"Subscriber {subscriber.GetType().FullName} did not return a {typeof(Task)}.");

            await task;
        }
        catch (Exception ex)
        {
            var exceptionHandler = serviceProvider.GetService(exceptionHandlerType);
            if (exceptionHandler == null)
                return;

            var result = handleExceptionAsync.Invoke(exceptionHandler, [envelope.Event, context, ex, cancellationToken]);
            if (result is not Task task)
                throw new UnreachableException($"Subscriber exception handler {exceptionHandlerType.FullName} did not return a {typeof(Task)}.");

            await task;
        }
    }
}