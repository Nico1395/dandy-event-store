namespace DandyEventStore;

public static class ReadOnlyEventStoreExtensions
{
    public static async Task<TAggregate?> ReplayAggregateAsync<TAggregate>(this IReadOnlyEventStore eventStore, string streamId, long? toVersion, DateTime? toTimestamp, CancellationToken cancellationToken)
        where TAggregate : class
    {
        return (TAggregate?)await eventStore.ReplayAggregateAsync(typeof(TAggregate), streamId, toVersion, toTimestamp, cancellationToken);
    }

    public static async Task<object?> ReplayAggregateAsync(this IReadOnlyEventStore eventStore, Type aggregateType, string streamId, long? toVersion, CancellationToken cancellationToken)
    {
        return await eventStore.ReplayAggregateAsync(aggregateType, streamId, toVersion, toTimestamp: null, cancellationToken);
    }

    public static async Task<(Snapshot? Snapshot, Envelope[] Stream)> ReplayStreamAsync(this IReadOnlyEventStore eventStore, string streamId, long? toVersion, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        var snapshot = await eventStore.GetLastSnapshotAsync(streamId, toVersion, cancellationToken);
        var stream = await eventStore.GetStreamAsync(
            streamId,
            fromVersion: snapshot?.Version + 1,
            toVersion: toVersion,
            fromTimestamp: null,
            toTimestamp: toTimestamp,
            cancellationToken);

        return (snapshot, stream);
    }

    public static Task<(Snapshot? Snapshot, Envelope[] Stream)> ReplayStreamAsync(this IReadOnlyEventStore eventStore, string streamId, long? toVersion, CancellationToken cancellationToken)
    {
        return eventStore.ReplayStreamAsync(streamId, toVersion, toTimestamp: null, cancellationToken);
    }
}