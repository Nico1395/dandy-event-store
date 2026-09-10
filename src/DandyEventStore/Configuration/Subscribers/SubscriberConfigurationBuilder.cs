using DandyEventStore.Subscribers;

namespace DandyEventStore.Configuration.Subscribers;

public sealed class SubscriberConfigurationBuilder(Type subscriberType, Type eventType)
{
    private readonly SubscriberConfiguration _configuration = new SubscriberConfiguration
    {
        Key = subscriberType.Name,
        AbstractionType = typeof(ISubscriber<>).MakeGenericType(eventType),
        RuntimeType = subscriberType,
        EventType = eventType,
    };

    public SubscriberConfigurationBuilder WithKey(string key)
    {
        _configuration.Key = key;
        return this;
    }
    
    public SubscriberConfigurationBuilder WithMode(SubscriberMode mode)
    {
        _configuration.Mode = mode;
        return this;
    }

    internal SubscriberConfiguration Build()
    {
        return _configuration;
    }
}