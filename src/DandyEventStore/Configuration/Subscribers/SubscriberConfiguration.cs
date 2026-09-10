using DandyEventStore.Subscribers;

namespace DandyEventStore.Configuration.Subscribers;

public sealed class SubscriberConfiguration
{
    public string Key { get; internal set; } = string.Empty;
    public required Type AbstractionType { get; init; }
    public required Type RuntimeType { get; init; }
    public required Type EventType { get; init; }
    public SubscriberMode Mode { get; internal set; } = SubscriberMode.Async;
}