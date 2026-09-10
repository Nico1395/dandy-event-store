namespace DandyEventStore.Persistence;

public interface IEventRepository
{
    Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken);
    Task<RawEnvelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken);
    Task InsertAsync(string streamId, RawEnvelope[] envelopes, CancellationToken cancellationToken);
}