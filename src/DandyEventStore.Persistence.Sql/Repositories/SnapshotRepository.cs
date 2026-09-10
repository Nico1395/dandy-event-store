using DandyEventStore.Persistence.Entities;
using Dapper;

namespace DandyEventStore.Persistence.Sql.Repositories;

internal sealed class SnapshotRepository(
    SqlStrings sqlStrings,
    IReadOnlyUnitOfWorkContext unitOfWorkContext) : ISnapshotRepository
{
    public async Task<SnapshotEntity?> GetLastSnapshotAsync(string streamId, long version, CancellationToken cancellationToken)
    {
        var raw = await unitOfWorkContext.Connection.QueryAsync<SnapshotEntity>(new CommandDefinition(
            sqlStrings.GetLastSnapshot,
            new { StreamId = streamId, Version = version },
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));

        return raw.FirstOrDefault();
    }

    public async Task InsertAsync(SnapshotEntity snapshotEntity, CancellationToken cancellationToken)
    {
        await unitOfWorkContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.StoreSnapshot,
            snapshotEntity,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }
}