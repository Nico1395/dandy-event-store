using DandyEventStore.Outbox;
using Dapper;

namespace DandyEventStore.Persistence.Sql;

internal sealed class OutboxRepository(
    SqlStrings sqlStrings,
    UnitOfWorkContext connectionContext) : IOutboxRepository
{
    public async Task<RawOutboxEnvelope[]> GetEnvelopesAsync(CancellationToken cancellationToken)
    {
        var rows = await connectionContext.Connection.QueryAsync<RawOutboxEnvelopeRow>(new CommandDefinition(
            sqlStrings.GetOutboxEnvelopes,
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));

        return RowsToEnvelopes(rows).ToArray();
    }

    public async Task InsertEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        var parameters = events.Select(e => new
        {
            e.StreamId,
            e.Payload,
            e.Version,
            e.Timestamp,
            e.EventKey,
        });

        await connectionContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.InsertOutboxEnvelopes,
            parameters,
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task DeleteEnvelopesAsync(RawOutboxEnvelope[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        var parameters = events.Select(e => new
        {
            e.StreamId,
            e.Version,
        });

        await connectionContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.DeleteOutboxEnvelopes,
            parameters,
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task InsertConsumersAsync(RawOutboxEnvelopeConsumer[] consumers, CancellationToken cancellationToken)
    {
        if (consumers.Length == 0)
            return;

        var parameters = consumers.Select(c => new
        {
            c.StreamId,
            c.Version,
            c.ConsumerKey,
            Type = (short)c.Type,
            c.ConsumedAt,
            c.FailedAt,
        });

        await connectionContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.InsertOutboxEnvelopeConsumers,
            parameters,
            transaction: connectionContext.Transaction,
            cancellationToken: cancellationToken));
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