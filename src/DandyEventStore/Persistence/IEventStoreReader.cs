namespace DandyEventStore.Persistence;

public interface IEventStoreReader
{
    Task<long> GetCurrentVersionAsync(string streamId, CancellationToken cancellationToken);
    Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken);
    Task<Snapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken);
}