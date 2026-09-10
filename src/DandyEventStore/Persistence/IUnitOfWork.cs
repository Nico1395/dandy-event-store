namespace DandyEventStore.Persistence;

public interface IUnitOfWork
{
    IEnvelopeRepository Envelopes { get; }
    ISnapshotRepository Snapshots { get; }
    IOutboxRepository Outbox { get; }
    Task CommitAsync(CancellationToken cancellationToken);
}