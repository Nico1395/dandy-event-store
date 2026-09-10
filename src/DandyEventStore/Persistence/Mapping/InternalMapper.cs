using DandyEventStore.Configuration;
using DandyEventStore.Outbox;
using DandyEventStore.Persistence.Entities;
using DandyEventStore.Serialization;

namespace DandyEventStore.Persistence.Mapping;

internal static class InternalMapper
{
    public static IEnumerable<EnvelopeEntity> MapToEntity(ISerializer serializer, IEnumerable<Envelope> envelopes)
    {
        return envelopes.Select(e => new EnvelopeEntity
        {
            StreamId = e.StreamId,
            Payload = serializer.Serialize(e.Event, e.RuntimeType),
            Version = e.Version,
            Timestamp = e.Timestamp,
            EventKey = e.EventKey,
        });
    }

    public static IEnumerable<Envelope> MapFromEntity(EventStoreConfiguration eventStoreConfiguration, ISerializer serializer, IEnvelopeFactory envelopeFactory, IEnumerable<EnvelopeEntity> envelopeEntities)
    {
        return envelopeEntities.Select(r =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(r.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {r.EventKey} is not configured.");

            var @event = serializer.Deserialize(r.Payload, configuration.RuntimeType);
            if (@event == null)
                throw new InvalidOperationException($"Failed to deserialize event {r.EventKey} from payload.");

            return envelopeFactory.Create(r.StreamId, @event, r.Version);
        });
    }

    public static IEnumerable<OutboxEnvelope> MapFromEntity(EventStoreConfiguration eventStoreConfiguration, ISerializer serializer, IEnumerable<OutboxEnvelopeEntity> envelopeEntities)
    {
        return envelopeEntities.Select(r =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(r.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {r.EventKey} is not configured.");

            var deserialized = serializer.Deserialize(r.Payload, configuration.RuntimeType);
            if (deserialized == null)
                throw new InvalidOperationException($"Failed to deserialize event {r.EventKey} from payload.");

            return new OutboxEnvelope
            {
                StreamId = r.StreamId,
                Event = deserialized,
                Version = r.Version,
                Timestamp = r.Timestamp,
                EventKey = r.EventKey,
                RuntimeType = configuration.RuntimeType,
                Consumers = r.Consumers.Select(c => new OutboxEnvelopeConsumer
                {
                    StreamId = c.StreamId,
                    Version = c.Version,
                    ConsumerKey = c.ConsumerKey,
                    Type = c.Type,
                    ConsumedAt = c.ConsumedAt,
                    FailedAt = c.FailedAt,
                }).ToList(),
            };
        });
    }

    public static IEnumerable<OutboxEnvelopeEntity> MapToEntity(EventStoreConfiguration eventStoreConfiguration, ISerializer serializer, IEnumerable<OutboxEnvelope> envelopes)
    {
        return envelopes.Select(e =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(e.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {e.EventKey} is not configured.");

            return new OutboxEnvelopeEntity
            {
                StreamId = e.StreamId,
                Payload = serializer.Serialize(e.Event, configuration.RuntimeType),
                Version = e.Version,
                Timestamp = e.Timestamp,
                EventKey = e.EventKey,
                Consumers = MapToEntity(e.Consumers).ToList(),
            };
        });
    }

    public static IEnumerable<OutboxEnvelopeConsumerEntity> MapToEntity(IEnumerable<OutboxEnvelopeConsumer> consumers)
    {
        return consumers.Select(c => new OutboxEnvelopeConsumerEntity
        {
            StreamId = c.StreamId,
            Version = c.Version,
            ConsumerKey = c.ConsumerKey,
            Type = c.Type,
            ConsumedAt = c.ConsumedAt,
            FailedAt = c.FailedAt,
        });
    }
}