namespace DandyEventStore.Configuration.Events;

public sealed class EventsConfigurationBuilder
{
    private readonly EventsConfiguration _configuration = new();
    
    public EventsConfigurationBuilder AddEvent<TEvent>(Action<EventConfigurationBuilder<TEvent>> builderAction)
        where TEvent : class
    {
        var builder = new EventConfigurationBuilder<TEvent>();
        builderAction(builder);
        var configuration = builder.Build();

        _configuration.EventConfigsByType[configuration.RuntimeType] = configuration;
        _configuration.EventConfigsByKey[configuration.Key] = configuration;

        return this;
    }

    public EventsConfigurationBuilder WithDefaultLifetime(TimeSpan lifetime)
    {
        _configuration.DefaultLifetime = lifetime;
        return this;
    }

    internal EventsConfiguration Build()
    {
        return _configuration;
    }
}