namespace DandyEventStore.Outbox;

public interface IOutbox
{
    Task PublishAsync(OutboxEnvelope[] outboxEnvelopes, CancellationToken cancellationToken);
    Task NotifyInlineConsumersAsync(OutboxEnvelope[] outboxEnvelopes, CancellationToken cancellationToken);
    Task CheckAndProcessAsync(CancellationToken cancellationToken);
}