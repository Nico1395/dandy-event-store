namespace DandyEventStore.Subscribers;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SubscriberAttribute : Attribute
{
    public SubscriberMode Mode { get; init; } = SubscriberMode.Async;
    public string? Key { get; init; }
}