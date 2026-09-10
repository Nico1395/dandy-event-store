using System.Data;
using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SqlEventRepository : IEventRepository
{
    private readonly SqlStrings _sqlStrings;
    private readonly IDbConnectionFactory? _dbConnectionFactory;
    private readonly IDbConnection? _connection;
    private readonly IDbTransaction? _transaction;

    public SqlEventRepository(SqlStrings sqlStrings, IDbConnectionFactory dbConnectionFactory)
    {
        _sqlStrings = sqlStrings;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public SqlEventRepository(SqlStrings sqlStrings, IDbConnection connection, IDbTransaction transaction)
    {
        _sqlStrings = sqlStrings;
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken)
    {
        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            _sqlStrings.GetStreamVersion,
            new { StreamId = streamId },
            transaction: _transaction,
            cancellationToken: cancellationToken));
    }

    public async Task<RawEnvelope[]> GetStreamAsync(string streamId, long? fromVersion, long? toVersion, DateTime? fromTimestamp, DateTime? toTimestamp, CancellationToken cancellationToken)
    {
        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        var raw = await connection.QueryAsync<RawEnvelope>(new CommandDefinition(
            _sqlStrings.GetStream,
            new
            {
                StreamId = streamId,
                FromVersion = fromVersion,
                ToVersion = toVersion,
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp,
            },
            transaction: _transaction,
            cancellationToken: cancellationToken));
        
        return raw.ToArray();
    }

    public async Task InsertAsync(string streamId, RawEnvelope[] envelopes, CancellationToken cancellationToken)
    {
        if (envelopes.Length == 0)
            return;

        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        var parameters = envelopes.Select(e => new
        {
            e.StreamId,
            e.Version,
            e.Timestamp,
            e.EventKey,
            e.Payload,
        });

        await connection.ExecuteAsync(new CommandDefinition(
            _sqlStrings.InsertEnvelope,
            parameters,
            transaction: _transaction,
            cancellationToken: cancellationToken));
    }

    private IDbConnection? OpenConnection()
    {
        return _connection == null ? _dbConnectionFactory!.CreateAndOpen() : null;
    }
}