namespace DandyEventStore.Events;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EventAttribute : Attribute
{
    public string? Key { get; init; }
}