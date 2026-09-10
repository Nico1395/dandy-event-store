namespace DandyEventStore;

public static class ReadOnlyEventStoreExtensions
{
    public static async Task<TAggregate?> ReplayAggregateAsync<TAggregate>(this IReadOnlyEventStore eventStore, string streamId, long? version, DateTime? timestamp, CancellationToken cancellationToken)
    {
        return (TAggregate?)await eventStore.ReplayAggregateAsync(typeof(TAggregate), streamId, version, timestamp, cancellationToken);
    }
}