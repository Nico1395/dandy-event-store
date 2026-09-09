namespace DandyEventStore.Events.Configurations;

public sealed class EventConfigurationBuilder<TEvent>
    where TEvent : class
{
    private readonly EventConfiguration _configuration = new()
    {
        Key = typeof(TEvent).Name,
        RuntimeType = typeof(TEvent),
    };

    public EventConfigurationBuilder<TEvent> WithKey(string key)
    {
        _configuration.Key = key;
        return this;
    }
    
    public EventConfigurationBuilder<TEvent> WithLifetime(TimeSpan lifetime)
    {
        _configuration.Lifetime = lifetime;
        return this;
    }

    internal EventConfiguration Build()
    {
        return _configuration;
    }
}