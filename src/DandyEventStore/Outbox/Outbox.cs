using DandyEventStore.Configuration;
using DandyEventStore.Persistence;
using DandyEventStore.Serialization;
using DandyEventStore.Subscribers;

namespace DandyEventStore.Outbox;

internal sealed class Outbox(
    IServiceProvider serviceProvider,
    EventStoreConfiguration eventStoreConfiguration,
    ISubscriberManager subscriberManager,
    IEventStoreSerializer eventStoreSerializer,
    IOutboxEnvelopeRepository outboxEnvelopeRepository) : IOutbox
{
    public async Task PublishAsync(string streamId, IEnumerable<Envelope> envelopes, CancellationToken cancellationToken)
    {
        try
        {
            var outboxEnvelopes = envelopes.Select(OutboxEnvelope.Create).ToArray();
            await outboxEnvelopeRepository.AddEnvelopesAsync(GetRaw(outboxEnvelopes).ToArray(), cancellationToken);

            foreach (var outboxEnvelope in outboxEnvelopes)
                await subscriberManager.NotifySyncSubscribersAsync(outboxEnvelope, cancellationToken);

            var consumers = outboxEnvelopes.SelectMany(e => e.Consumers);
            await outboxEnvelopeRepository.AddConsumersAsync(GetRaw(consumers).ToArray(), cancellationToken);
        }
        catch (Exception ex)
        {
            eventStoreConfiguration.OnOutboxPublishException?.Invoke(serviceProvider, ex);
            throw;
        }
    }

    public Task IterateAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<RawOutboxEnvelope> GetRaw(IEnumerable<OutboxEnvelope> envelopes)
    {
        return envelopes.Select(e =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(e.EventKey, out var configuration))
                throw new InvalidOperationException($"Event type {e.EventKey} is not configured.");

            return new RawOutboxEnvelope
            {
                StreamId = e.StreamId,
                Payload = eventStoreSerializer.Serialize(e.Event, configuration.RuntimeType),
                Version = e.Version,
                Timestamp = e.Timestamp,
                EventKey = e.EventKey,
                Consumers = GetRaw(e.Consumers).ToList(),
            };
        });
    }

    private IEnumerable<RawOutboxEnvelopeConsumer> GetRaw(IEnumerable<OutboxEnvelopeConsumer> consumers)
    {
        return consumers.Select(c => new RawOutboxEnvelopeConsumer
        {
            StreamId = c.StreamId,
            Version = c.Version,
            ConsumerKey = c.ConsumerKey,
            Type = c.Type,
        });
    }
}