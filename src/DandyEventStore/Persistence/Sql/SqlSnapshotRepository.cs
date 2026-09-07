using DandyEventStore.Persistence.Connections;
using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SqlSnapshotRepository(
    SqlStrings sqlStrings,
    IConnectionFactory connectionFactory) : ISnapshotRepository
{
    public async Task<RawSnapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateAndOpen();
        var raw = await connection.QueryAsync<RawSnapshot>(sqlStrings.GetLastSnapshot, new { StreamId = streamId });

        return raw.FirstOrDefault();
    }

    public async Task StoreAsync(RawSnapshot snapshot, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateAndOpen();
        await connection.ExecuteAsync(sqlStrings.StoreSnapshot, snapshot);
    }
}