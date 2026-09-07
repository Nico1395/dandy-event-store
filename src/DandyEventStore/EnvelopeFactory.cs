namespace DandyEventStore;

public class EnvelopeFactory : IEnvelopeFactory
{
    public Envelope Create(string streamId, object @event, long version)
    {
        // TODO:
        // - Allow mapping of event type to key

        var runtimeType = @event.GetType();
        var envelope = new Envelope
        {
            StreamId = streamId,
            Event = @event,
            Timestamp = DateTime.UtcNow,
            Version = version,
            EventType = runtimeType.Name,
            RuntimeType = runtimeType,
        };

        return envelope;
    }
}