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
                                                   WHERE {Tables.Snapshots.StreamId} = @StreamId AND (@ToVersion IS NULL OR {Tables.Snapshots.Version} <= @ToVersion)
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

    public override string GetOutboxEnvelopes => $"""
                                                        SELECT
                                                            e.{Tables.OutboxEnvelopes.StreamId} AS StreamId,
                                                            e.{Tables.OutboxEnvelopes.Payload} AS Payload,
                                                            e.{Tables.OutboxEnvelopes.Version} AS Version,
                                                            e.{Tables.OutboxEnvelopes.Timestamp} AS Timestamp,
                                                            e.{Tables.OutboxEnvelopes.EventKey} AS EventKey,
                                                            c.{Tables.OutboxEnvelopeConsumers.StreamId} AS ConsumerStreamId,
                                                            c.{Tables.OutboxEnvelopeConsumers.Version} AS ConsumerVersion,
                                                            c.{Tables.OutboxEnvelopeConsumers.ConsumerKey} AS ConsumerKey,
                                                            c.{Tables.OutboxEnvelopeConsumers.Type} AS ConsumerType,
                                                            c.{Tables.OutboxEnvelopeConsumers.ConsumedAt} AS ConsumedAt,
                                                            c.{Tables.OutboxEnvelopeConsumers.FailedAt} AS FailedAt
                                                        FROM {Schema.Name}.{Tables.OutboxEnvelopes.Table} e
                                                        LEFT JOIN {Schema.Name}.{Tables.OutboxEnvelopeConsumers.Table} c
                                                            ON c.{Tables.OutboxEnvelopeConsumers.StreamId} = e.{Tables.OutboxEnvelopes.StreamId}
                                                            AND c.{Tables.OutboxEnvelopeConsumers.Version} = e.{Tables.OutboxEnvelopes.Version}
                                                    """;

    public override string InsertOutboxEnvelopes => $"""
                                                         INSERT INTO {Schema.Name}.{Tables.OutboxEnvelopes.Table} (
                                                             {Tables.OutboxEnvelopes.StreamId},
                                                             {Tables.OutboxEnvelopes.Payload},
                                                             {Tables.OutboxEnvelopes.Version},
                                                             {Tables.OutboxEnvelopes.Timestamp},
                                                             {Tables.OutboxEnvelopes.EventKey})
                                                         VALUES (
                                                             @StreamId,
                                                             @Payload,
                                                             @Version,
                                                             @Timestamp,
                                                             @EventKey)
                                                     """;

    public override string DeleteOutboxEnvelopes => $"""
                                                         DELETE FROM {Schema.Name}.{Tables.OutboxEnvelopeConsumers.Table}
                                                         WHERE {Tables.OutboxEnvelopeConsumers.StreamId} = @StreamId
                                                         AND {Tables.OutboxEnvelopeConsumers.Version} = @Version;
                                                         DELETE FROM {Schema.Name}.{Tables.OutboxEnvelopes.Table}
                                                         WHERE {Tables.OutboxEnvelopes.StreamId} = @StreamId
                                                         AND {Tables.OutboxEnvelopes.Version} = @Version
                                                     """;

    public override string InsertOutboxEnvelopeConsumers => $"""
                                                                INSERT INTO {Schema.Name}.{Tables.OutboxEnvelopeConsumers.Table} (
                                                                    {Tables.OutboxEnvelopeConsumers.StreamId},
                                                                    {Tables.OutboxEnvelopeConsumers.Version},
                                                                    {Tables.OutboxEnvelopeConsumers.ConsumerKey},
                                                                    {Tables.OutboxEnvelopeConsumers.Type},
                                                                    {Tables.OutboxEnvelopeConsumers.ConsumedAt},
                                                                    {Tables.OutboxEnvelopeConsumers.FailedAt})
                                                                VALUES (
                                                                    @StreamId,
                                                                    @Version,
                                                                    @ConsumerKey,
                                                                    @Type,
                                                                    @ConsumedAt,
                                                                    @FailedAt)
                                                            """;

    public override string UpdateOutboxEnvelopeConsumers => $"""
                                                                UPDATE {Schema.Name}.{Tables.OutboxEnvelopeConsumers.Table}
                                                                SET
                                                                    {Tables.OutboxEnvelopeConsumers.ConsumedAt} = @ConsumedAt,
                                                                    {Tables.OutboxEnvelopeConsumers.FailedAt} = @FailedAt
                                                                WHERE {Tables.OutboxEnvelopeConsumers.StreamId} = @StreamId
                                                                AND {Tables.OutboxEnvelopeConsumers.Version} = @Version
                                                                AND {Tables.OutboxEnvelopeConsumers.ConsumerKey} = @ConsumerKey
                                                            """;
}