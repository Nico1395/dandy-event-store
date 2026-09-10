namespace DandyEventStore;

public interface IEventStore : IReadOnlyEventStore
{
    Task AppendAsync(Type? aggregateType, string streamId, object[] events, CancellationToken cancellationToken);
}