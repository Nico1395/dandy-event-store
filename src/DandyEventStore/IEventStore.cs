namespace DandyEventStore;

public interface IEventStore : IReadOnlyEventStore
{
    Task AppendAsync(string streamId, object[] events, CancellationToken cancellationToken);
}