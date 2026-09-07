using System.Diagnostics;
using DandyEventStore.Aggregates;
using DandyEventStore.Aggregates.Configuration;
using DandyEventStore.Persistence;
using DandyEventStore.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore;

public class EventStore(
    AggregatesConfiguration aggregatesConfiguration,
    IServiceProvider serviceProvider,
    IEnvelopeFactory envelopeFactory,
    IProjector projector,
    IEventStoreReader eventStoreReader,
    IEventStoreWriter eventStoreWriter) : IEventStore
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

    public Task<Envelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        return eventStoreReader.GetStreamAsync(streamId, fromVersion, toVersion, fromTimestamp, toTimestamp, cancellationToken);
    }

    public async Task AppendAsync(string streamId, object[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or whitespace.", nameof(streamId));

        var currentVersion = await eventStoreReader.GetCurrentVersionAsync(streamId, cancellationToken);
        var envelopes = events.Select(e => envelopeFactory.Create(streamId, e, currentVersion++)).ToArray();

        await eventStoreWriter.WriteAsync(streamId, envelopes, cancellationToken);
        await projector.ProjectAsync(this, envelopes, ProjectionMode.Immediate, cancellationToken);

        // TODO: Snapshots, if configured for the stream/aggregate
    }

    private async Task<(Snapshot? snapshot, Envelope[] Stream)> GetSnapshotAndStreamAsync(AggregateConfiguration aggregateConfig, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        Snapshot? snapshot = null;

        if (aggregateConfig.UseSnapshots())
        {
            snapshot = await eventStoreReader.GetLastSnapshotAsync(streamId, version ?? 0, cancellationToken);
            if (snapshot != null && snapshot.AggregateType != aggregateConfig.AggregateType)
                throw new InvalidOperationException($"Snapshot for stream {streamId} is of type {snapshot.Aggregate.GetType().FullName}, but expected {aggregateConfig.AggregateType.FullName}.");
        }

        var stream = await eventStoreReader.GetStreamAsync(
            streamId,
            fromVersion: snapshot?.Version,
            toVersion: version,
            fromTimestamp: null,
            toTimestamp: timestamp,
            cancellationToken);

        return (snapshot, stream);
    }
}