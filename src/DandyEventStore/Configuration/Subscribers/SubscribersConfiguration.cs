using System.Collections.Concurrent;
using System.Reflection;
using DandyEventStore.Subscribers;

namespace DandyEventStore.Configuration.Subscribers;

public sealed class SubscribersConfiguration
{
    internal ConcurrentDictionary<Type, SubscriberConfiguration> ByType { get; } = [];
    internal ConcurrentDictionary<string, SubscriberConfiguration> ByKey { get; } = [];
    internal ConcurrentDictionary<Type, List<SubscriberConfiguration>> ByEventType { get; } = [];

    public IReadOnlyDictionary<Type, SubscriberConfiguration> SubscribersByType => ByType;
    public IReadOnlyDictionary<string, SubscriberConfiguration> SubscribersByKey => ByKey;
    public IReadOnlyDictionary<Type, List<SubscriberConfiguration>> SubscribersByEventType => ByEventType;

    internal SubscriberConfiguration GetOrAddSubscriberConfiguration(Type subscriberType)
    {
        return ByType.GetOrAdd(subscriberType, type =>
        {
            var subscriberInterface = subscriberType
                .GetInterfaces()
                .FirstOrDefault(interfaceType =>
                    interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == typeof(ISubscriber<>));

            if (subscriberInterface is null)
                throw new ArgumentException($"Subscriber of type '{subscriberType}' does not implement '{typeof(ISubscriber<>)}'.", nameof(subscriberType));

            var attribute = type.GetCustomAttribute<SubscriberAttribute>();
            var eventType = subscriberInterface.GetGenericArguments()[0];
            var configuration = new SubscriberConfiguration
            {
                Key = attribute?.Key ?? subscriberType.Name,
                AbstractionType = typeof(ISubscriber<>).MakeGenericType(eventType),
                RuntimeType = subscriberType,
                EventType = eventType,
                Mode = attribute?.Mode ?? SubscriberMode.Async,
            };

            ByKey[configuration.Key] = configuration;
            var subscriberTypes = ByEventType.GetOrAdd(eventType, _ => []);
            subscriberTypes.Add(configuration);

            return configuration;
        });
    }
}