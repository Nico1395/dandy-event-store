namespace DandyEventStore.Configuration.Outbox;

public sealed class OutboxConfigurationBuilder
{
    private readonly OutboxConfiguration _configuration = new();

    public OutboxConfigurationBuilder WithInterval(TimeSpan interval)
    {
        _configuration.Interval = interval;
        return this;
    }

    public OutboxConfigurationBuilder WithDefaultEventLifetime(TimeSpan lifetime)
    {
        _configuration.DefaultEventLifetime = lifetime;
        return this;
    }

    public OutboxConfigurationBuilder DisableDaemon()
    {
        _configuration.DaemonEnabled = false;
        return this;
    }

    internal OutboxConfiguration Build()
    {
        return _configuration;
    }
}