using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SqlEventRepository(
    SqlStrings sqlStrings,
    IDbConnectionFactory dbConnectionFactory) : IEventRepository
{
    public async Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateAndOpen();
        return await connection.ExecuteScalarAsync<long>(sqlStrings.GetStreamVersion, new { StreamId = streamId });
    }

    public async Task<RawEnvelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateAndOpen();
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

        using var connection = dbConnectionFactory.CreateAndOpen();
        var parameters = envelopes.Select(e => new
        {
            StreamId = e.StreamId,
            Version = e.Version,
            Timestamp = e.Timestamp,
            EventKey = e.EventKey,
            Payload = e.Payload,
        });

        await connection.ExecuteAsync(sqlStrings.StoreEnvelope, parameters);
    }
}