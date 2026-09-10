using System.Diagnostics;
using DandyEventStore.Configuration;
using DandyEventStore.Configuration.Aggregates;
using DandyEventStore.Outbox;
using DandyEventStore.Persistence;
using DandyEventStore.Persistence.Entities;
using DandyEventStore.Persistence.Mapping;
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
    IOutbox outbox,
    IUnitOfWork unitOfWork) : IEventStore
{
    public async Task<object?> ReplayAggregateAsync(Type aggregateType, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        var configuration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfiguration(aggregateType);
        var (snapshot, stream) = await GetSnapshotAndStreamAsync(configuration, streamId, version, timestamp, cancellationToken);

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

    public async Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        var envelopeEntities = await unitOfWork.Envelopes.GetStreamAsync(streamId, fromVersion, toVersion, fromTimestamp, toTimestamp, cancellationToken);
        var envelopes = InternalMapper.MapFromEntity(
            eventStoreConfiguration,
            serializer,
            envelopeFactory,
            envelopeEntities);

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
        var envelopeEntities = InternalMapper.MapToEntity(serializer, envelopes).ToArray();

        // Insert events
        await unitOfWork.Envelopes.InsertAsync(streamId, envelopeEntities, cancellationToken);

        // Publish outbox envelopes
        var outboxEnvelopes = envelopes.Select(OutboxEnvelope.Create).ToArray();
        await outbox.PublishAsync(outboxEnvelopes, cancellationToken);

        // Create and insert a snapshot if configured
        await CreateSnapshotAsync(aggregateType, streamId, envelopes, currentVersion, cancellationToken);

        // Commit transaction so event-store and outbox are in sync. Consumers should not be in this transaction.
        await unitOfWork.CommitAsync(cancellationToken);

        // Notify inline subscribers
        await outbox.NotifyInlineConsumersAsync(outboxEnvelopes, cancellationToken);
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

    private async Task CreateSnapshotAsync(Type? aggregateType, string streamId, Envelope[] envelopes, long currentVersion, CancellationToken cancellationToken)
    {
        if (aggregateType == null || envelopes.Length == 0)
            return;

        var aggregateConfiguration = eventStoreConfiguration.Aggregates.GetOrAddAggregateConfiguration(aggregateType);
        var versionAfterAppend = envelopes.OrderByDescending(e => e.Version).First().Version;

        if (aggregateConfiguration.ShouldCreateSnapshot(currentVersion, versionAfterAppend))
        {
            var aggregate = await ReplayAggregateAsync(aggregateType, streamId, null, null, cancellationToken);
            if (aggregate == null)
                throw new InvalidOperationException($"Failed to replay aggregate {aggregateType.FullName} from stream {streamId} to create snapshot.");

            var snapshotEntity = new SnapshotEntity
            {
                StreamId = streamId,
                Version = versionAfterAppend,
                Timestamp = DateTime.UtcNow,
                Payload = serializer.Serialize(aggregate, aggregateType),
                AggregateKey = aggregateConfiguration.Key,
            };

            await unitOfWork.Snapshots.InsertAsync(snapshotEntity, cancellationToken);
        }
    }
}