namespace DandyEventStore.Events;

public sealed class EventConfiguration
{
    public string Key { get; internal set; } = string.Empty;
    public required Type RuntimeType { get; init; }
}