using System.Data;
using DandyEventStore.Outbox;
using Dapper;

namespace DandyEventStore.Persistence.Sql;

public class SqlOutboxEnvelopeRepository : IOutboxEnvelopeRepository
{
    private readonly SqlStrings _sqlStrings;
    private readonly IDbConnectionFactory? _dbConnectionFactory;
    private readonly IDbConnection? _connection;
    private readonly IDbTransaction? _transaction;

    public SqlOutboxEnvelopeRepository(SqlStrings sqlStrings, IDbConnectionFactory dbConnectionFactory)
    {
        _sqlStrings = sqlStrings;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public SqlOutboxEnvelopeRepository(SqlStrings sqlStrings, IDbConnection connection, IDbTransaction transaction)
    {
        _sqlStrings = sqlStrings;
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<RawOutboxEnvelope[]> GetEnvelopesAsync(CancellationToken cancellationToken)
    {
        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        var rows = await connection.QueryAsync<RawOutboxEnvelopeRow>(
            new CommandDefinition(_sqlStrings.GetOutboxEnvelopes, transaction: _transaction, cancellationToken: cancellationToken));

        return RowsToEnvelopes(rows).ToArray();
    }

    public async Task InsertEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        var parameters = events.Select(e => new
        {
            e.StreamId,
            e.Payload,
            e.Version,
            e.Timestamp,
            e.EventKey,
        });

        await connection.ExecuteAsync(new CommandDefinition(
            _sqlStrings.InsertOutboxEnvelopes,
            parameters,
            transaction: _transaction,
            cancellationToken: cancellationToken));
    }

    public async Task DeleteEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        var parameters = events.Select(e => new
        {
            e.StreamId,
            e.Version,
        });

        await connection.ExecuteAsync(new CommandDefinition(
            _sqlStrings.DeleteOutboxEnvelopes,
            parameters,
            transaction: _transaction,
            cancellationToken: cancellationToken));
    }

    public async Task InsertConsumersAsync(RawOutboxEnvelopeConsumer[] consumers, CancellationToken cancellationToken)
    {
        if (consumers.Length == 0)
            return;

        using var ownedConnection = OpenConnection();
        var connection = _connection ?? ownedConnection;
        var parameters = consumers.Select(c => new
        {
            c.StreamId,
            c.Version,
            c.ConsumerKey,
            Type = (short)c.Type,
            c.ConsumedAt,
            c.FailedAt,
        });

        await connection.ExecuteAsync(new CommandDefinition(
            _sqlStrings.InsertOutboxEnvelopeConsumers,
            parameters,
            transaction: _transaction,
            cancellationToken: cancellationToken));
    }

    private IDbConnection? OpenConnection()
    {
        return _connection == null ? _dbConnectionFactory!.CreateAndOpen() : null;
    }

    private static IEnumerable<RawOutboxEnvelope> RowsToEnvelopes(IEnumerable<RawOutboxEnvelopeRow> rows)
    {
        return rows
            .GroupBy(row => new
            {
                row.StreamId,
                row.Version,
            })
            .Select(group =>
            {
                var envelope = group.First();
                return new RawOutboxEnvelope
                {
                    StreamId = group.Key.StreamId,
                    Payload = envelope.Payload,
                    Version = group.Key.Version,
                    Timestamp = envelope.Timestamp,
                    EventKey = envelope.EventKey,
                    Consumers = group
                        .Where(row => row.ConsumerKey is not null)
                        .Select(row => new RawOutboxEnvelopeConsumer
                        {
                            StreamId = row.ConsumerStreamId!,
                            Version = row.ConsumerVersion!.Value,
                            ConsumerKey = row.ConsumerKey!,
                            Type = (OutboxEventConsumerType)row.ConsumerType!.Value,
                            ConsumedAt = row.ConsumedAt,
                            FailedAt = row.FailedAt,
                        })
                        .ToList(),
                };
            });
    }

    private sealed class RawOutboxEnvelopeRow
    {
        public required string StreamId { get; init; }
        public required object Payload { get; init; }
        public required long Version { get; init; }
        public required DateTime Timestamp { get; init; }
        public required string EventKey { get; init; }
        public string? ConsumerStreamId { get; init; }
        public long? ConsumerVersion { get; init; }
        public string? ConsumerKey { get; init; }
        public short? ConsumerType { get; init; }
        public DateTime? ConsumedAt { get; init; }
        public DateTime? FailedAt { get; init; }
    }
}