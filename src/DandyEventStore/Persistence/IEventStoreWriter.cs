namespace DandyEventStore.Persistence;

public interface IEventStoreWriter
{
    Task WriteAsync(string streamId, Envelope[] envelopes, CancellationToken cancellationToken);
}