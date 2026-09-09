namespace DandyEventStore.Outbox;

public interface IOutbox
{
    Task PublishAsync(string streamId, IEnumerable<Envelope> envelopes, CancellationToken cancellationToken);
    Task IterateAsync(CancellationToken cancellationToken);
}