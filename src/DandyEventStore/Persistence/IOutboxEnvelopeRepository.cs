using DandyEventStore.Outbox;

namespace DandyEventStore.Persistence;

public interface IOutboxEnvelopeRepository
{
    Task<RawOutboxEnvelope[]> GetEnvelopesAsync(CancellationToken cancellationToken); 
    Task AddEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken);
    Task DeleteEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken);
    Task AddConsumersAsync(RawOutboxEnvelopeConsumer[] consumers, CancellationToken cancellationToken);
}