using DandyEventStore.Outbox;
using DandyEventStore.Persistence.Entities;

namespace DandyEventStore.Persistence;

public interface IOutboxRepository
{
    Task<OutboxEnvelopeEntity[]> GetEnvelopesAsync(CancellationToken cancellationToken); 
    Task InsertEnvelopesAsync(OutboxEnvelopeEntity[] events, CancellationToken cancellationToken);
    Task DeleteEnvelopesAsync(OutboxEnvelopeEntity[] events, CancellationToken cancellationToken);
    Task InsertConsumersAsync(OutboxEnvelopeConsumerEntity[] consumers, CancellationToken cancellationToken);
    Task UpdateConsumersAsync(OutboxEnvelopeConsumerEntity[] consumers, CancellationToken cancellationToken);
}