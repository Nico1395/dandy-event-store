using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SnapshotRepository(
    SqlStrings sqlStrings,
    UnitOfWorkContext connectionContext) : ISnapshotRepository
{
    public async Task<RawSnapshot?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken)
    {
        var raw = await connectionContext.Connection.QueryAsync<RawSnapshot>(new CommandDefinition(
            sqlStrings.GetLastSnapshot,
            new { StreamId = streamId, Version = version },
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));

        return raw.FirstOrDefault();
    }

    public async Task InsertAsync(RawSnapshot snapshot, CancellationToken cancellationToken)
    {
        await connectionContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.StoreSnapshot,
            snapshot,
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
    }
}