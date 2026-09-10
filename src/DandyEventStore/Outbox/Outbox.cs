using DandyEventStore.Configuration;
using DandyEventStore.Persistence;
using DandyEventStore.Persistence.Mapping;
using DandyEventStore.Serialization;
using DandyEventStore.Subscribers;

namespace DandyEventStore.Outbox;

internal sealed class Outbox(
    EventStoreConfiguration eventStoreConfiguration,
    ISerializer serializer,
    ISubscriptionManager subscriptionManager,
    IUnitOfWork unitOfWork) : IOutbox
{
    public async Task PublishAsync(OutboxEnvelope[] outboxEnvelopes, CancellationToken cancellationToken)
    {
        var outboxEnvelopesEntities = InternalMapper.MapToEntity(eventStoreConfiguration, serializer, outboxEnvelopes).ToArray();
        await unitOfWork.Outbox.InsertEnvelopesAsync(outboxEnvelopesEntities, cancellationToken);
    }

    public async Task NotifyInlineConsumersAsync(OutboxEnvelope[] outboxEnvelopes, CancellationToken cancellationToken)
    {
        foreach (var outboxEnvelope in outboxEnvelopes)
        {
            await subscriptionManager.NotifySubscribersAsync(
                eventStore: null,
                outboxEnvelope, 
                modes: [SubscriberMode.Inline],
                cancellationToken);
        }

        var outboxConsumers = outboxEnvelopes.SelectMany(e => e.Consumers);
        var outboxConsumerEntities = InternalMapper.MapToEntity(outboxConsumers).ToArray();

        await unitOfWork.Outbox.InsertConsumersAsync(outboxConsumerEntities, cancellationToken);
    }

    public async Task CheckAndProcessAsync(CancellationToken cancellationToken)
    {
        var rawEnvelopes = await unitOfWork.Outbox.GetEnvelopesAsync(cancellationToken);

        // Remove expired envelopes first, we don't want to consume those again
        var expired = rawEnvelopes.Where(e =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(e.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {e.EventKey} is not configured.");

            var lifeTime = configuration.Lifetime ?? eventStoreConfiguration.Events.DefaultLifetime;
            return DateTime.UtcNow >= e.Timestamp + lifeTime;
        }).ToArray();

        if (expired.Length > 0)
        {
            await unitOfWork.Outbox.DeleteEnvelopesAsync(expired, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        var envelopes = InternalMapper.MapFromEntity(eventStoreConfiguration, serializer, rawEnvelopes.Except(expired)).ToArray();
        foreach (var envelope in envelopes)
        {
            // The subscription manager filters out subscribers that have already successfully consumed the envelope
            // using the envelope's consumers and their statuses.

            await subscriptionManager.NotifySubscribersAsync(
                eventStore: null,
                envelope,
                [SubscriberMode.Inline, SubscriberMode.Async],
                cancellationToken);
        }

        var consumers = envelopes.SelectMany(e => e.Consumers).ToArray();

        var consumersToInsert = consumers.Where(c => c.IsNew);
        var insertEntities = InternalMapper.MapToEntity(consumersToInsert).ToArray();
        await unitOfWork.Outbox.InsertConsumersAsync(insertEntities, cancellationToken);

        var consumersToUpdate = consumers.Where(c => !c.IsNew);
        var updateEntities = InternalMapper.MapToEntity(consumersToUpdate).ToArray();
        await unitOfWork.Outbox.UpdateConsumersAsync(updateEntities, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}