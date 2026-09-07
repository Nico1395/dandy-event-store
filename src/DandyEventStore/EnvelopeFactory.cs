namespace DandyEventStore;

public class EnvelopeFactory : IEnvelopeFactory
{
    public Envelope Create(string streamId, object @event, long version)
    {
        // TODO:
        // - Allow mapping of event type to key

        var envelope = new Envelope
        {
            StreamId = streamId,
            Event = @event,
            Timestamp = DateTime.UtcNow,
            Version = version,
            EventTypeKey = @event.GetType().Name,
        };

        return envelope;
    }
}