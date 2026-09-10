using System.Diagnostics;
using DandyEventStore.Aggregates;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Configuration;
using DandyEventStore.Outbox;
using DandyEventStore.Persistence;
using DandyEventStore.Serialization;
using DandyEventStore.Subscribers;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore;

internal sealed class EventStore(
    EventStoreConfiguration eventStoreConfiguration,
    IServiceProvider serviceProvider,
    IEnvelopeFactory envelopeFactory,
    ISubscriberManager subscriberManager,
    ISerializer serializer,
    IOutboxEnvelopeRepository outboxEnvelopeRepository,
    IEventRepository eventRepository,
    ISnapshotRepository snapshotRepository) : IEventStore
{
    public async Task<TAggregate?> ReplayAggregateAsync<TAggregate>(string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
        where TAggregate : class
    {
        var configuration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfig(typeof(TAggregate));
        var (snapshot, stream) = await GetSnapshotAndStreamAsync(configuration, streamId, version, timestamp, cancellationToken);
        if (stream.Length == 0)
            return null;

        if (configuration.FactoryFunc != null)
            return configuration.FactoryFunc(snapshot?.Aggregate, stream) as TAggregate;

        var aggregateFactory = serviceProvider.GetRequiredService<IAggregateFactory<TAggregate>>();
        return aggregateFactory.Create(snapshot?.Aggregate as TAggregate, stream);
    }

    public async Task<object?> ReplayAggregateAsync(Type aggregateType, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        var configuration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfig(aggregateType);
        var (snapshot, stream) = await GetSnapshotAndStreamAsync(configuration, streamId, version, timestamp, cancellationToken);
        if (stream.Length == 0)
            return null;

        if (configuration.FactoryFunc != null)
            return configuration.FactoryFunc(snapshot?.Aggregate, stream);

        var factoryType = typeof(IAggregateFactory<>).MakeGenericType(aggregateType);
        var factory = serviceProvider.GetRequiredService(factoryType);

        var create = factoryType.GetMethod(nameof(IAggregateFactory<>.Create)) ?? throw new UnreachableException();
        return create.Invoke(factory, [snapshot?.Aggregate, stream]);
    }

    public async Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        var rawEnvelopes = await eventRepository.GetStreamAsync(streamId, fromVersion, toVersion, fromTimestamp, toTimestamp, cancellationToken);
        var envelopes = rawEnvelopes.Select(r =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(r.EventKey, out var configuration))
                throw new InvalidOperationException($"Event type {r.EventKey} is not configured.");

            var @event = serializer.Deserialize(r.Payload, configuration.RuntimeType);
            if (@event == null)
                throw new InvalidOperationException($"Failed to deserialize event {r.EventKey} from payload.");

            return envelopeFactory.Create(r.StreamId, @event, r.Version);
        });

        return envelopes.ToArray();
    }

    public async Task AppendAsync(string streamId, object[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or whitespace.", nameof(streamId));

        // 1. Create envelopes
        var currentVersion = await eventRepository.GetStreamVersionAsync(streamId, cancellationToken);
        var envelopes = events.Select(e => envelopeFactory.Create(streamId, e, currentVersion++)).ToArray();
        var rawEnvelopes = envelopes.Select(e => new RawEnvelope
        {
            StreamId = e.StreamId,
            Payload = serializer.Serialize(e.Event, e.RuntimeType),
            Version = e.Version,
            Timestamp = e.Timestamp,
            EventKey = e.EventKey,
        });

        // 2. Create a transaction if persistence implementations use SQL databases
        var transactionFactory = serviceProvider.GetService<ITransactionFactory>();
        await using var transaction = transactionFactory?.Create();
        var transactionalEventRepository = transaction?.EventRepository ?? eventRepository;
        var transactionalOutboxRepository = transaction?.OutboxEnvelopeRepository ?? outboxEnvelopeRepository;

        // 3. Insert events
        await transactionalEventRepository.InsertAsync(streamId, rawEnvelopes.ToArray(), cancellationToken);

        // 4. Insert outbox envelopes
        var outboxEnvelopes = envelopes.Select(OutboxEnvelope.Create).ToArray();
        await transactionalOutboxRepository.InsertEnvelopesAsync(GetRaw(outboxEnvelopes).ToArray(), cancellationToken);

        // 5. Create and insert a snapshot if configured
        // TODO

        // 6. Commit transaction so event-store and outbox are in sync. Consumers should not be in this transaction.
        if (transaction != null)
            await transaction.CommitAsync(cancellationToken);

        // 7. Notify sync subscribers
        foreach (var outboxEnvelope in outboxEnvelopes)
            await subscriberManager.NotifySyncSubscribersAsync(outboxEnvelope, cancellationToken);

        // 8. Insert an outbox envelope consumer for every sync subscriber
        await using var consumerTransaction = transactionFactory?.Create();
        var consumerOutboxRepository = consumerTransaction?.OutboxEnvelopeRepository ?? outboxEnvelopeRepository;

        var consumers = outboxEnvelopes.SelectMany(e => e.Consumers);
        await consumerOutboxRepository.InsertConsumersAsync(GetRaw(consumers).ToArray(), cancellationToken);

        if (consumerTransaction != null)
            await consumerTransaction.CommitAsync(cancellationToken);
    }

    private async Task<(Snapshot? snapshot, Envelope[] Stream)> GetSnapshotAndStreamAsync(AggregateConfiguration configuration, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        Snapshot? snapshot = null;
        if (configuration.UseSnapshots())
        {
            var rawSnapshot = await snapshotRepository.GetLastSnapshotAsync(streamId, version ?? 0, cancellationToken);
            if (rawSnapshot != null)
            {
                var aggregate = serializer.Deserialize(rawSnapshot.Payload, configuration.RuntimeType);
                if (aggregate != null)
                {
                    snapshot = new Snapshot
                    {
                        StreamId = rawSnapshot.StreamId,
                        Version = rawSnapshot.Version,
                        Timestamp = rawSnapshot.Timestamp,
                        Aggregate = aggregate,
                        AggregateKey = rawSnapshot.AggregateKey,
                        RuntimeType = configuration.RuntimeType,
                    };
                }
            }

            if (snapshot != null && snapshot.RuntimeType != configuration.RuntimeType)
                throw new InvalidOperationException($"Snapshot for stream {streamId} is of type {snapshot.Aggregate.GetType().FullName}, but expected {configuration.RuntimeType.FullName}.");
        }

        var stream = await GetStreamAsync(
            streamId,
            fromVersion: snapshot?.Version,
            toVersion: version,
            fromTimestamp: null,
            toTimestamp: timestamp,
            cancellationToken);

        return (snapshot, stream);
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
                Payload = serializer.Serialize(e.Event, configuration.RuntimeType),
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