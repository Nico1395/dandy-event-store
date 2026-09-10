using DandyEventStore.Outbox;

namespace DandyEventStore.Persistence;

public interface IOutboxRepository
{
    Task<RawOutboxEnvelope[]> GetEnvelopesAsync(CancellationToken cancellationToken); 
    Task InsertEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken);
    Task DeleteEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken);
    Task InsertConsumersAsync(RawOutboxEnvelopeConsumer[] consumers, CancellationToken cancellationToken);
}