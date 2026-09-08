using DandyEventStore.Configuration;

namespace DandyEventStore;

public class EnvelopeFactory(EventStoreConfiguration eventStoreConfiguration) : IEnvelopeFactory
{
    public Envelope Create(string streamId, object @event, long version)
    {
        var configuration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfig(@event.GetType());
        var envelope = new Envelope
        {
            StreamId = streamId,
            Event = @event,
            Timestamp = DateTime.UtcNow,
            Version = version,
            EventKey = configuration.Key,
            RuntimeType = configuration.RuntimeType,
        };

        return envelope;
    }
}