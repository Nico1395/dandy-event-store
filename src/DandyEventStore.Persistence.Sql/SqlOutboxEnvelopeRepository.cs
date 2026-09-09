using DandyEventStore.Outbox;
using Dapper;

namespace DandyEventStore.Persistence.Sql;

public class SqlOutboxEnvelopeRepository(
    SqlStrings sqlStrings,
    IDbConnectionFactory dbConnectionFactory) : IOutboxEnvelopeRepository
{
    public async Task<RawOutboxEnvelope[]> GetEnvelopesAsync(CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateAndOpen();
        var raw = await connection.QueryAsync<RawOutboxEnvelope>(sqlStrings.GetOutboxEnvelopes);

        return raw.ToArray();
    }

    public async Task AddEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        using var connection = dbConnectionFactory.CreateAndOpen();
        var parameters = events.Select(s => new
        {

        });

        await connection.ExecuteAsync(sqlStrings.InsertOutboxEnvelopes, parameters);
        
        // TODO
    }

    public async Task DeleteEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;
        
        // TODO
        
    }

    public async Task AddConsumersAsync(RawOutboxEnvelopeConsumer[] consumers, CancellationToken cancellationToken)
    {
        if (consumers.Length == 0)
            return;
        
        using var connection = dbConnectionFactory.CreateAndOpen();
        // TODO
        
    }
}