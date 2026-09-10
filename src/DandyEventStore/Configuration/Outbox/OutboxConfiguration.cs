namespace DandyEventStore.Configuration.Outbox;

public sealed class OutboxConfiguration
{
    public TimeSpan Interval { get; internal set; } = TimeSpan.FromSeconds(3);
}