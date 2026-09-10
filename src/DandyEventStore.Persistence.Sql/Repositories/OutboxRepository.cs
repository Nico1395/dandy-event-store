using DandyEventStore.Outbox;
using DandyEventStore.Persistence.Entities;
using Dapper;

namespace DandyEventStore.Persistence.Sql.Repositories;

internal sealed class OutboxRepository(
    SqlStrings sqlStrings,
    IReadOnlyUnitOfWorkContext unitOfWorkContext) : IOutboxRepository
{
    public async Task<OutboxEnvelopeEntity[]> GetEnvelopesAsync(CancellationToken cancellationToken)
    {
        var rows = await unitOfWorkContext.Connection.QueryAsync<RawOutboxEnvelopeRow>(new CommandDefinition(
            sqlStrings.GetOutboxEnvelopes,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));

        return RowsToEnvelopes(rows).ToArray();
    }

    public async Task InsertEnvelopesAsync(OutboxEnvelopeEntity[] events, CancellationToken cancellationToken)
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

        await unitOfWorkContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.InsertOutboxEnvelopes,
            parameters,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task DeleteEnvelopesAsync(OutboxEnvelopeEntity[] events, CancellationToken cancellationToken)
    {
        if (events.Length == 0)
            return;

        var parameters = events.Select(e => new
        {
            e.StreamId,
            e.Version,
        });

        await unitOfWorkContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.DeleteOutboxEnvelopes,
            parameters,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task InsertConsumersAsync(OutboxEnvelopeConsumerEntity[] consumers, CancellationToken cancellationToken)
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

        await unitOfWorkContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.InsertOutboxEnvelopeConsumers,
            parameters,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }

    public async Task UpdateConsumersAsync(OutboxEnvelopeConsumerEntity[] consumers, CancellationToken cancellationToken)
    {
        if (consumers.Length == 0)
            return;

        var parameters = consumers.Select(c => new
        {
            c.StreamId,
            c.Version,
            c.ConsumerKey,
            c.ConsumedAt,
            c.FailedAt,
        });

        await unitOfWorkContext.Connection.ExecuteAsync(new CommandDefinition(
            sqlStrings.UpdateOutboxEnvelopeConsumers,
            parameters,
            transaction: unitOfWorkContext.Transaction,
            cancellationToken: cancellationToken));
    }

    private static IEnumerable<OutboxEnvelopeEntity> RowsToEnvelopes(IEnumerable<RawOutboxEnvelopeRow> rows)
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
                return new OutboxEnvelopeEntity
                {
                    StreamId = group.Key.StreamId,
                    Payload = envelope.Payload,
                    Version = group.Key.Version,
                    Timestamp = envelope.Timestamp,
                    EventKey = envelope.EventKey,
                    Consumers = group
                        .Where(row => row.ConsumerKey is not null)
                        .Select(row => new OutboxEnvelopeConsumerEntity
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
        public required string Payload { get; init; }
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