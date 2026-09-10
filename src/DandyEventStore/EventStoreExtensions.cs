namespace DandyEventStore;

public static class EventStoreExtensions
{
    public static Task AppendAsync(this IEventStore eventStore, Type aggregateType, string streamId, object @event, CancellationToken cancellationToken)
    {
        return eventStore.AppendAsync(aggregateType, streamId, [@event], cancellationToken);
    }

    public static Task AppendAsync(this IEventStore eventStore, string streamId, object[] events, CancellationToken cancellationToken)
    {
        return eventStore.AppendAsync(null, streamId, events, cancellationToken);
    }

    public static Task AppendAsync(this IEventStore eventStore, string streamId, object @event, CancellationToken cancellationToken)
    {
        return eventStore.AppendAsync(null, streamId, [@event], cancellationToken);
    }

    public static Task AppendAsync<TAggregate>(this IEventStore eventStore, string streamId, object[] events, CancellationToken cancellationToken)
        where TAggregate : class
    {
        return eventStore.AppendAsync(typeof(TAggregate), streamId, events, cancellationToken);
    }

    public static Task AppendAsync<TAggregate>(this IEventStore eventStore, string streamId, object @event, CancellationToken cancellationToken)
        where TAggregate : class
    {
        return eventStore.AppendAsync(typeof(TAggregate), streamId, [@event], cancellationToken);
    }
}