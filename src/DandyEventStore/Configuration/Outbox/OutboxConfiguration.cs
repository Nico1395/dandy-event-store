namespace DandyEventStore.Configuration.Outbox;

public sealed class OutboxConfiguration
{
    public TimeSpan Interval { get; internal set; } = TimeSpan.FromSeconds(3);
    public TimeSpan DefaultEventLifetime { get; internal set; } = TimeSpan.FromMinutes(5);
    public bool DaemonEnabled { get; internal set; } = true;
}