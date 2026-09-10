namespace DandyEventStore;

public interface IReadOnlyEventStore
{
    Task<object?> ReplayAggregateAsync(Type aggregateType, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken);
    Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken);
}