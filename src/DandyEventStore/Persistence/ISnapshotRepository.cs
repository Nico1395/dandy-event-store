namespace DandyEventStore.Persistence;

public interface ISnapshotRepository
{
    Task<RawSnapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken);
    Task StoreAsync(RawSnapshot snapshot, CancellationToken cancellationToken);
}