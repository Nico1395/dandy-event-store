using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SnapshotRepository(
    SqlStrings sqlStrings,
    UnitOfWorkContext connectionContext) : ISnapshotRepository
{
    public async Task<RawSnapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken)
    {
        var raw = await connectionContext.Connection.QueryAsync<RawSnapshot>(sqlStrings.GetLastSnapshot, new { StreamId = streamId });

        return raw.FirstOrDefault();
    }

    public async Task InsertAsync(RawSnapshot snapshot, CancellationToken cancellationToken)
    {
        await connectionContext.Connection.ExecuteAsync(sqlStrings.StoreSnapshot, snapshot);
    }
}