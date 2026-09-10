using DandyEventStore.Persistence.Entities;
using Dapper;

namespace DandyEventStore.Persistence.Sql.Repositories;

internal sealed class EnvelopeRepository(
    SqlStrings sqlStrings,
    IReadOnlyUnitOfWorkContext unitOfWorkContext) : IEnvelopeRepository
{
    public async Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken)
    {
        return await unitOfWorkContext.Connection.ExecuteScalarAsync<long>(new CommandDefinition(
            sqlStrings.GetStreamVersion,
            new { StreamId = streamId },
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task<EnvelopeEntity[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        var raw = await unitOfWorkContext.Connection.QueryAsync<EnvelopeEntity>(new CommandDefinition(
            sqlStrings.GetStream,
            new
            {
                StreamId = streamId,
                FromVersion = fromVersion,
                ToVersion = toVersion,
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp,
            },
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
        
        return raw.ToArray();
    }

    public async Task InsertAsync(string streamId, EnvelopeEntity[] envelopes, CancellationToken cancellationToken)
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

        await unitOfWorkContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.InsertEnvelope,
            parameters,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }
}