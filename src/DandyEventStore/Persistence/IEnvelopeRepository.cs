using DandyEventStore.Persistence.Entities;

namespace DandyEventStore.Persistence;

public interface IEnvelopeRepository
{
    Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken);
    Task<EnvelopeEntity[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken);
    Task InsertAsync(string streamId, EnvelopeEntity[] envelopes, CancellationToken cancellationToken);
}