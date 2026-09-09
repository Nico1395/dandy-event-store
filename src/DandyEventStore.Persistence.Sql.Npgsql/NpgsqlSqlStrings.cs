using DandyEventStore.Persistence.Sql.Constants;

namespace DandyEventStore.Persistence.Sql.Npgsql;

internal sealed class NpgsqlSqlStrings : SqlStrings
{
    public override string GetStreamVersion => $"""
                                                    SELECT MAX(Version)
                                                    FROM {Schema.Name}.{Tables.Envelopes.Table}
                                                    WHERE {Tables.Envelopes.StreamId} = @StreamId
                                                """;

    public override string GetStream => $"""
                                              SELECT
                                                  {Tables.Envelopes.StreamId},
                                                  {Tables.Envelopes.Payload},
                                                  {Tables.Envelopes.Version},
                                                  {Tables.Envelopes.Timestamp},
                                                  {Tables.Envelopes.EventKey}
                                              FROM {Schema.Name}.{Tables.Envelopes.Table}
                                              WHERE {Tables.Envelopes.StreamId} = @StreamId
                                              AND (@FromVersion IS NULL OR {Tables.Envelopes.Version} >= @FromVersion)
                                              AND (@ToVersion IS NULL OR {Tables.Envelopes.Version} <= @ToVersion)
                                              AND (@FromTimestamp IS NULL OR {Tables.Envelopes.Timestamp} >= @FromTimestamp)
                                              AND (@ToTimestamp IS NULL OR {Tables.Envelopes.Timestamp} <= @ToTimestamp)
                                          """;

    public override string InsertEnvelope => $"""
                                                 INSERT INTO {Schema.Name}.{Tables.Envelopes.Table} (
                                                     {Tables.Envelopes.StreamId},
                                                     {Tables.Envelopes.Payload},
                                                     {Tables.Envelopes.Version},
                                                     {Tables.Envelopes.Timestamp},
                                                     {Tables.Envelopes.EventKey})
                                                 VALUES (
                                                     @StreamId,
                                                     @Payload,
                                                     @Version,
                                                     @Timestamp,
                                                     @EventKey)
                                             """;

    public override string GetLastSnapshot => $"""
                                                   SELECT
                                                       {Tables.Snapshots.StreamId},
                                                       {Tables.Snapshots.Payload},
                                                       {Tables.Snapshots.Version},
                                                       {Tables.Snapshots.Timestamp},
                                                       {Tables.Snapshots.AggregateKey}
                                                   FROM {Schema.Name}.{Tables.Snapshots.Table}
                                                   WHERE {Tables.Snapshots.Version} <= @Version && {Tables.Snapshots.StreamId} = @StreamId
                                                   ORDER BY {Tables.Snapshots.Version} DESC
                                                   LIMIT 1
                                               """;

    public override string StoreSnapshot => $"""
                                                 INSERT INTO {Schema.Name}.{Tables.Snapshots.Table} (
                                                     {Tables.Snapshots.StreamId},
                                                     {Tables.Snapshots.Payload},
                                                     {Tables.Snapshots.Version},
                                                     {Tables.Snapshots.Timestamp},
                                                     {Tables.Snapshots.AggregateKey})
                                                 VALUES (
                                                     @StreamId,
                                                     @Payload,
                                                     @Version,
                                                     @Timestamp,
                                                     @AggregateKey)
                                             """;

    public override string GetOutboxEnvelopes { get; }
    public override string InsertOutboxEnvelopes { get; }
    public override string DeleteOutboxEnvelopes { get; }
    public override string InsertOutboxEnvelopeConsumers { get; }
}