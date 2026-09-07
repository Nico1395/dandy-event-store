using DandyEventStore.Persistence.Connections;
using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SqlEventRepository(
    SqlStrings sqlStrings,
    IConnectionFactory connectionFactory) : IEventRepository
{
    public async Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateAndOpen();
        return await connection.ExecuteScalarAsync<long>(sqlStrings.GetStreamVersion, new { StreamId = streamId });
    }

    public async Task<RawEnvelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateAndOpen();
        var raw = await connection.QueryAsync<RawEnvelope>(sqlStrings.GetStream, new
        {
            StreamId = streamId,
            FromVersion = fromVersion,
            ToVersion = toVersion,
            FromTimestamp = fromTimestamp,
            ToTimestamp = toTimestamp,
        });

        return raw.ToArray();
    }

    public async Task StoreAsync(string streamId, RawEnvelope[] envelopes, CancellationToken cancellationToken)
    {
        if (envelopes.Length == 0)
            return;

        using var connection = connectionFactory.CreateAndOpen();
        await connection.ExecuteAsync(sqlStrings.StoreEnvelope, envelopes.ToArray());
    }
}