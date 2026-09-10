using DandyEventStore.Subscribers;

namespace DandyEventStore.Configuration.Subscribers;

public sealed class SubscribersConfigurationBuilder
{
    private readonly SubscribersConfiguration _configuration = new();

    public SubscribersConfigurationBuilder AddSubscriber(Type subscriberType, Action<SubscriberConfigurationBuilder> builderAction)
    {
        if (!subscriberType.IsAssignableTo(typeof(ISubscriber<>)))
            throw new ArgumentException($"Subscriber of type '{subscriberType}' does not implement '{typeof(ISubscriber<>)}'.", nameof(subscriberType));

        var eventType = subscriberType.GetGenericArguments()[0];
        var builder = new SubscriberConfigurationBuilder(subscriberType, eventType);
        builderAction(builder);
        var configuration = builder.Build();

        _configuration.ByType[configuration.RuntimeType] = configuration;
        _configuration.ByKey[configuration.Key] = configuration;

        return this;
    }

    internal SubscribersConfiguration Build()
    {
        return _configuration;
    }
}