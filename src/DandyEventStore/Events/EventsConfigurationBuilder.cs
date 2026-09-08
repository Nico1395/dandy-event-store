namespace DandyEventStore.Events;

public sealed class EventsConfigurationBuilder
{
    private readonly List<EventConfiguration> _events = [];
    
    public EventsConfigurationBuilder AddEvent<TEvent>(Action<EventConfigurationBuilder<TEvent>> builderAction)
        where TEvent : class
    {
        var builder = new EventConfigurationBuilder<TEvent>();
        builderAction(builder);
        var configuration = builder.Build();

        _events.Add(configuration);

        return this;
    }

    internal EventsConfiguration Build()
    {
        return new EventsConfiguration
        {
            Events = _events.ToArray(),
        };
    }
}