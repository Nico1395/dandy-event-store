namespace DandyEventStore;

public interface IEnvelopeFactory
{
    Envelope Create(string streamId, object @event, long version);
}