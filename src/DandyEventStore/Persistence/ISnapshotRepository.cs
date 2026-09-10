using DandyEventStore.Persistence.Entities;

namespace DandyEventStore.Persistence;

public interface ISnapshotRepository
{
    Task<SnapshotEntity?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken);
    Task InsertAsync(SnapshotEntity snapshotEntity, CancellationToken cancellationToken);
}