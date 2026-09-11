namespace DandyEventStore;

public interface IReadOnlyEventStore
{
    Task<object?> ReplayAggregateAsync(Type aggregateType, string streamId, long? toVersion, DateTime? toTimestamp, CancellationToken cancellationToken);
    Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken);
    Task<Snapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken);
}