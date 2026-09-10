using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class EnvelopeRepository(
    SqlStrings sqlStrings,
    UnitOfWorkContext connectionContext) : IEnvelopeRepository
{
    public async Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken)
    {
        return await connectionContext.Connection.ExecuteScalarAsync<long>(new CommandDefinition(
            sqlStrings.GetStreamVersion,
            new { StreamId = streamId },
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task<RawEnvelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        var raw = await connectionContext.Connection.QueryAsync<RawEnvelope>(new CommandDefinition(
            sqlStrings.GetStream,
            new
            {
                StreamId = streamId,
                FromVersion = fromVersion,
                ToVersion = toVersion,
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp,
            },
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
        
        return raw.ToArray();
    }

    public async Task InsertAsync(string streamId, RawEnvelope[] envelopes, CancellationToken cancellationToken)
    {
        if (envelopes.Length == 0)
            return;

        var parameters = envelopes.Select(e => new
        {
            e.StreamId,
            e.Version,
            e.Timestamp,
            e.EventKey,
            e.Payload,
        });

        await connectionContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.InsertEnvelope,
            parameters,
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
    }
}