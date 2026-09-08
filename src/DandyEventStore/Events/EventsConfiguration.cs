namespace DandyEventStore.Events;

public sealed class EventsConfiguration
{
    public required EventConfiguration[] Events { get; init; }
}