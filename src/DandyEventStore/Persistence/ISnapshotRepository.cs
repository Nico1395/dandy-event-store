namespace DandyEventStore.Persistence;

public interface ISnapshotRepository
{
    Task<Snapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken);
    Task StoreAsync(Snapshot snapshot, CancellationToken cancellationToken);
}