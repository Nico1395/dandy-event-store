using DandyEventStore.Persistence.Constants;
using DandyEventStore.Persistence.Sql;

namespace DandyEventStore.Persistence.Npgsql;

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
                                                 {Tables.Envelopes.EventType}
                                             FROM {Schema.Name}.{Tables.Envelopes.Table}
                                             WHERE {Tables.Envelopes.StreamId} = @StreamId
                                         """;

    public override string StoreEnvelope => $"""
                                                 INSERT INTO {Schema.Name}.{Tables.Envelopes.Table} (
                                                     {Tables.Envelopes.StreamId},
                                                     {Tables.Envelopes.Payload},
                                                     {Tables.Envelopes.Version},
                                                     {Tables.Envelopes.Timestamp},
                                                     {Tables.Envelopes.EventType})
                                                 VALUES (
                                                     @StreamId,
                                                     @Payload,
                                                     @Version,
                                                     @Timestamp,
                                                     @EventType)
                                             """;

    public override string GetLastSnapshot => $"""
                                                   SELECT
                                                       {Tables.Snapshots.StreamId},
                                                       {Tables.Snapshots.Payload},
                                                       {Tables.Snapshots.Version},
                                                       {Tables.Snapshots.Timestamp},
                                                       {Tables.Snapshots.AggregateType}
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
                                                     {Tables.Snapshots.AggregateType})
                                                 VALUES (
                                                     @StreamId,
                                                     @Payload,
                                                     @Version,
                                                     @Timestamp,
                                                     @AggregateType)
                                             """;
}