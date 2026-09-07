namespace DandyEventStore.Persistence;

public interface IEventRepository
{
    Task<long> GetCurrentVersionAsync(string streamId, CancellationToken cancellationToken);
    Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken);
    Task StoreAsync(string streamId, Envelope[] envelopes, CancellationToken cancellationToken);
}