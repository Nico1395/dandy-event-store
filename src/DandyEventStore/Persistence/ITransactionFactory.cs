namespace DandyEventStore.Persistence;

public interface ITransactionFactory
{
    ITransaction Create();
}

public interface ITransaction : IAsyncDisposable
{
    IEventRepository EventRepository { get; }
    IOutboxEnvelopeRepository OutboxEnvelopeRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken);
}