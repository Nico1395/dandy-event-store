using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SqlSnapshotRepository(
    SqlStrings sqlStrings,
    IDbConnectionFactory dbConnectionFactory) : ISnapshotRepository
{
    public async Task<RawSnapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateAndOpen();
        var raw = await connection.QueryAsync<RawSnapshot>(sqlStrings.GetLastSnapshot, new { StreamId = streamId });

        return raw.FirstOrDefault();
    }

    public async Task InsertAsync(RawSnapshot snapshot, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateAndOpen();
        await connection.ExecuteAsync(sqlStrings.StoreSnapshot, snapshot);
    }
}