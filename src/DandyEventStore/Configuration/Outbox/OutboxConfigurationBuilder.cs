namespace DandyEventStore.Configuration.Outbox;

public sealed class OutboxConfigurationBuilder
{
    private readonly OutboxConfiguration _configuration = new();

    public OutboxConfigurationBuilder WithInterval(TimeSpan interval)
    {
        _configuration.Interval = interval;
        return this;
    }
    
    internal OutboxConfiguration Build()
    {
        return _configuration;
    }
}