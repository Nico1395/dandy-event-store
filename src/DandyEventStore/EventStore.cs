using System.Diagnostics;
using DandyEventStore.Aggregates;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Persistence;
using DandyEventStore.Projections;
using DandyEventStore.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore;

public class EventStore(
    AggregatesConfiguration aggregatesConfiguration,
    IServiceProvider serviceProvider,
    IEnvelopeFactory envelopeFactory,
    IProjector projector,
    IEventStoreSerializer eventStoreSerializer,
    IEventRepository eventRepository,
    ISnapshotRepository snapshotRepository) : IEventStore
{
    public async Task<TAggregate?> ReplayAggregateAsync<TAggregate>(string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
        where TAggregate : class
    {
        var aggregateConfig = aggregatesConfiguration.GetOrAdd(typeof(TAggregate));
        var (snapshot, stream) = await GetSnapshotAndStreamAsync(aggregateConfig, streamId, version, timestamp, cancellationToken);
        if (stream.Length == 0)
            return null;

        if (aggregateConfig.FactoryFunc != null)
            return aggregateConfig.FactoryFunc(snapshot?.Aggregate, stream) as TAggregate;

        var aggregateFactory = serviceProvider.GetRequiredService<IAggregateFactory<TAggregate>>();
        return aggregateFactory.Create(snapshot?.Aggregate as TAggregate, stream);
    }

    public async Task<object?> ReplayAggregateAsync(Type aggregateType, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        var aggregateConfig = aggregatesConfiguration.GetOrAdd(aggregateType);
        var (snapshot, stream) = await GetSnapshotAndStreamAsync(aggregateConfig, streamId, version, timestamp, cancellationToken);
        if (stream.Length == 0)
            return null;

        if (aggregateConfig.FactoryFunc != null)
            return aggregateConfig.FactoryFunc(snapshot?.Aggregate, stream);

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
            var eventType = EventTypeProvider.Get(r.EventType);
            return new Envelope
            {
                StreamId = r.StreamId,
                Event = eventStoreSerializer.Deserialize(r.Payload, eventType),
                Version = r.Version,
                Timestamp = r.Timestamp,
                EventType = r.EventType,
                RuntimeType = eventType,
            };
        });

        return envelopes.ToArray();
    }

    public async Task AppendAsync(string streamId, object[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or whitespace.", nameof(streamId));

        var currentVersion = await eventRepository.GetStreamVersionAsync(streamId, cancellationToken);
        var envelopes = events.Select(e => envelopeFactory.Create(streamId, e, currentVersion++)).ToArray();

        // TODO: Maybe create raw envelopes directly
        var rawEnvelopes = envelopes.Select(e => new RawEnvelope
        {
            StreamId = e.StreamId,
            Payload = eventStoreSerializer.Serialize(e.Event),
            Version = e.Version,
            Timestamp = e.Timestamp,
            EventType = e.EventType,
        });

        await eventRepository.StoreAsync(streamId, rawEnvelopes.ToArray(), cancellationToken);
        await projector.ProjectAsync(this, envelopes, ProjectionMode.Immediate, cancellationToken);

        // TODO: Snapshots, if configured for the stream/aggregate
    }

    private async Task<(Snapshot? snapshot, Envelope[] Stream)> GetSnapshotAndStreamAsync(AggregateConfiguration aggregateConfig, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        Snapshot? snapshot = null;
        if (aggregateConfig.UseSnapshots())
        {
            var rawSnapshot = await snapshotRepository.GetLastSnapshotAsync(streamId, version ?? 0, cancellationToken);
            if (rawSnapshot != null)
            {
                snapshot = new Snapshot
                {
                    StreamId = rawSnapshot.StreamId,
                    Version = rawSnapshot.Version,
                    Timestamp = rawSnapshot.Timestamp,
                    Aggregate = eventStoreSerializer.Deserialize(rawSnapshot.Payload, aggregateConfig.AggregateType),
                };
            }

            if (snapshot != null && snapshot.AggregateType != aggregateConfig.AggregateType)
                throw new InvalidOperationException($"Snapshot for stream {streamId} is of type {snapshot.Aggregate.GetType().FullName}, but expected {aggregateConfig.AggregateType.FullName}.");
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
}