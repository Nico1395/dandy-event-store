using System.Diagnostics;
using DandyEventStore.Configuration;
using DandyEventStore.Configuration.Aggregates;
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
    ISubscriptionManager subscriptionManager,
    ISerializer serializer,
    IUnitOfWork unitOfWork) : IEventStore
{
    public async Task<object?> ReplayAggregateAsync(Type aggregateType, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        var configuration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfiguration(aggregateType);
        var (snapshot, stream) = await GetSnapshotAndStreamAsync(configuration, streamId, version, timestamp, cancellationToken);

        return ReplayAggregate(configuration, streamId, snapshot, stream);
    }

    public async Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        var rawEnvelopes = await unitOfWork.Envelopes.GetStreamAsync(streamId, fromVersion, toVersion, fromTimestamp, toTimestamp, cancellationToken);
        var envelopes = rawEnvelopes.Select(r =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(r.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {r.EventKey} is not configured.");

            var @event = serializer.Deserialize(r.Payload, configuration.RuntimeType);
            if (@event == null)
                throw new InvalidOperationException($"Failed to deserialize event {r.EventKey} from payload.");

            return envelopeFactory.Create(r.StreamId, @event, r.Version);
        });

        return envelopes.ToArray();
    }

    public async Task AppendAsync(Type? aggregateType, string streamId, object[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or whitespace.", nameof(streamId));

        // Fetch current version
        var currentVersion = await unitOfWork.Envelopes.GetStreamVersionAsync(streamId, cancellationToken);
        
        // Create envelopes
        var envelopeVersion = currentVersion;
        var envelopes = events.Select(e => envelopeFactory.Create(streamId, e, envelopeVersion++)).ToArray();
        var rawEnvelopes = envelopes.Select(e => new RawEnvelope
        {
            StreamId = e.StreamId,
            Payload = serializer.Serialize(e.Event, e.RuntimeType),
            Version = e.Version,
            Timestamp = e.Timestamp,
            EventKey = e.EventKey,
        });

        // Insert events
        await unitOfWork.Envelopes.InsertAsync(streamId, rawEnvelopes.ToArray(), cancellationToken);

        // Insert outbox envelopes
        var outboxEnvelopes = envelopes.Select(OutboxEnvelope.Create).ToArray();
        await unitOfWork.Outbox.InsertEnvelopesAsync(GetRaw(outboxEnvelopes).ToArray(), cancellationToken);

        // Create and insert a snapshot if configured
        await CreateSnapshotAsync(aggregateType, streamId, envelopes, currentVersion, cancellationToken);

        // Commit transaction so event-store and outbox are in sync. Consumers should not be in this transaction.
        await unitOfWork.CommitAsync(cancellationToken);

        // Notify inline subscribers
        foreach (var outboxEnvelope in outboxEnvelopes)
            await subscriptionManager.NotifySubscribersAsync(outboxEnvelope, [SubscriberMode.Inline], cancellationToken);

        // Save consumers for every subscriber
        var consumers = outboxEnvelopes.SelectMany(e => e.Consumers);
        await unitOfWork.Outbox.InsertConsumersAsync(GetRaw(consumers).ToArray(), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<(Snapshot? snapshot, Envelope[] Stream)> GetSnapshotAndStreamAsync(AggregateConfiguration configuration, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        Snapshot? snapshot = null;
        if (configuration.UseSnapshots())
        {
            var rawSnapshot = await unitOfWork.Snapshots.GetLastSnapshotAsync(streamId, version ?? 0, cancellationToken);
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
            fromVersion: snapshot?.Version + 1,
            toVersion: version,
            fromTimestamp: null,
            toTimestamp: timestamp,
            cancellationToken);

        return (snapshot, stream);
    }

    private object? ReplayAggregate(AggregateConfiguration configuration, string streamId, Snapshot? snapshot, Envelope[] stream)
    {
        var hasDuplicates = stream.GroupBy(e => e.Version).Any(c => c.Count() > 1);
        if (hasDuplicates)
            throw new InvalidOperationException($"Stream with ID '{streamId}' has duplicate events.");

        stream = stream.OrderBy(e => e.Version).ToArray();

        if (configuration.FactoryFunc != null)
            return configuration.FactoryFunc(snapshot?.Aggregate, stream);

        var factoryType = typeof(IAggregateFactory<>).MakeGenericType(configuration.RuntimeType);
        var factory = serviceProvider.GetRequiredService(factoryType);

        var create = factoryType.GetMethod(nameof(IAggregateFactory<>.Create)) ?? throw new UnreachableException();
        return create.Invoke(factory, [snapshot?.Aggregate, stream]);
    }

    private async Task CreateSnapshotAsync(Type? aggregateType, string streamId, Envelope[] envelopes, long currentVersion, CancellationToken cancellationToken)
    {
        if (aggregateType == null || envelopes.Length == 0)
            return;

        var aggregateConfiguration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfiguration(aggregateType);
        var versionAfterAppend = envelopes.OrderByDescending(e => e.Version).First().Version;

        if (aggregateConfiguration.ShouldCreateSnapshot(currentVersion, versionAfterAppend))
        {
            var (snapshot, stream) = await GetSnapshotAndStreamAsync(aggregateConfiguration, streamId, null, null, cancellationToken);
            var aggregate = ReplayAggregate(aggregateConfiguration, streamId, snapshot, stream);
            if (aggregate == null)
                throw new InvalidOperationException($"Failed to replay aggregate {aggregateType.FullName} from stream {streamId} to create snapshot.");

            var rawSnapshot = new RawSnapshot
            {
                StreamId = streamId,
                Version = versionAfterAppend,
                Timestamp = DateTime.UtcNow,
                Payload = serializer.Serialize(aggregate, aggregateType),
                AggregateKey = aggregateConfiguration.Key,
            };

            await unitOfWork.Snapshots.InsertAsync(rawSnapshot, cancellationToken);
        }
    }

    private IEnumerable<RawOutboxEnvelope> GetRaw(IEnumerable<OutboxEnvelope> envelopes)
    {
        return envelopes.Select(e =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(e.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {e.EventKey} is not configured.");

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
            ConsumedAt = c.ConsumedAt,
            FailedAt = c.FailedAt,
        });
    }
}