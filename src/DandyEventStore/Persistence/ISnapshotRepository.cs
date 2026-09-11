using DandyEventStore.Persistence.Entities;

namespace DandyEventStore.Persistence;

public interface ISnapshotRepository
{
    Task<SnapshotEntity?> GetLatestSnapshotAsync(string streamId, long? toVersion, CancellationToken cancellationToken);
    Task InsertAsync(SnapshotEntity snapshotEntity, CancellationToken cancellationToken);
}