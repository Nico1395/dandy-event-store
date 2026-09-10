namespace DandyEventStore.Outbox;

internal interface IOutbox
{
    Task PublishAsync(OutboxEnvelope[] outboxEnvelopes, CancellationToken cancellationToken);
    Task NotifyInlineConsumersAsync(OutboxEnvelope[] outboxEnvelopes, CancellationToken cancellationToken);
    Task ExecuteIntervalAsync(CancellationToken cancellationToken);
}