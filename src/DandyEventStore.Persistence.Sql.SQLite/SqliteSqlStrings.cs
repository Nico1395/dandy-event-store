using DandyEventStore.Persistence.Sql.Constants;

namespace DandyEventStore.Persistence.Sql.SQLite;

internal sealed class SqliteSqlStrings : SqlStrings
{
    public override string GetStreamVersion => $"""
                                                    SELECT MAX(Version)
                                                    FROM {Tables.Envelopes.Table}
                                                    WHERE {Tables.Envelopes.StreamId} = @StreamId
                                                """;

    public override string GetStream => $"""
                                              SELECT
                                                  {Tables.Envelopes.StreamId} AS StreamId,
                                                  {Tables.Envelopes.Payload} AS Payload,
                                                  {Tables.Envelopes.Version} AS Version,
                                                  {Tables.Envelopes.Timestamp} AS Timestamp,
                                                  {Tables.Envelopes.EventKey} AS EventKey
                                              FROM {Tables.Envelopes.Table}
                                              WHERE {Tables.Envelopes.StreamId} = @StreamId
                                              AND (@FromVersion IS NULL OR {Tables.Envelopes.Version} >= @FromVersion)
                                              AND (@ToVersion IS NULL OR {Tables.Envelopes.Version} <= @ToVersion)
                                              AND (@FromTimestamp IS NULL OR {Tables.Envelopes.Timestamp} >= @FromTimestamp)
                                              AND (@ToTimestamp IS NULL OR {Tables.Envelopes.Timestamp} <= @ToTimestamp)
                                          """;

    public override string InsertEnvelope => $"""
                                                 INSERT INTO {Tables.Envelopes.Table} (
                                                     {Tables.Envelopes.StreamId},
                                                     {Tables.Envelopes.Version},
                                                     {Tables.Envelopes.Timestamp},
                                                     {Tables.Envelopes.EventKey},
                                                     {Tables.Envelopes.Payload})
                                                 VALUES (
                                                     @StreamId,
                                                     @Version,
                                                     @Timestamp,
                                                     @EventKey,
                                                     @Payload)
                                             """;

    public override string GetLastSnapshot => $"""
                                                   SELECT
                                                       {Tables.Snapshots.StreamId} AS StreamId,
                                                       {Tables.Snapshots.Payload} AS Payload,
                                                       {Tables.Snapshots.Version} AS Version,
                                                       {Tables.Snapshots.Timestamp} AS Timestamp,
                                                       {Tables.Snapshots.AggregateKey} AS AggregateKey
                                                   FROM {Tables.Snapshots.Table}
                                                   WHERE {Tables.Snapshots.Version} <= @Version AND {Tables.Snapshots.StreamId} = @StreamId
                                                   ORDER BY {Tables.Snapshots.Version} DESC
                                                   LIMIT 1
                                               """;

    public override string StoreSnapshot => $"""
                                                 INSERT INTO {Tables.Snapshots.Table} (
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
                                                        FROM {Tables.OutboxEnvelopes.Table} e
                                                        LEFT JOIN {Tables.OutboxEnvelopeConsumers.Table} c
                                                            ON c.{Tables.OutboxEnvelopeConsumers.StreamId} = e.{Tables.OutboxEnvelopes.StreamId}
                                                            AND c.{Tables.OutboxEnvelopeConsumers.Version} = e.{Tables.OutboxEnvelopes.Version}
                                                    """;

    public override string InsertOutboxEnvelopes => $"""
                                                         INSERT INTO {Tables.OutboxEnvelopes.Table} (
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
                                                         DELETE FROM {Tables.OutboxEnvelopeConsumers.Table}
                                                         WHERE {Tables.OutboxEnvelopeConsumers.StreamId} = @StreamId
                                                         AND {Tables.OutboxEnvelopeConsumers.Version} = @Version;
                                                         DELETE FROM {Tables.OutboxEnvelopes.Table}
                                                         WHERE {Tables.OutboxEnvelopes.StreamId} = @StreamId
                                                         AND {Tables.OutboxEnvelopes.Version} = @Version
                                                     """;

    public override string InsertOutboxEnvelopeConsumers => $"""
                                                                INSERT INTO {Tables.OutboxEnvelopeConsumers.Table} (
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
                                                                UPDATE {Tables.OutboxEnvelopeConsumers.Table}
                                                                SET
                                                                    {Tables.OutboxEnvelopeConsumers.ConsumedAt} = @ConsumedAt,
                                                                    {Tables.OutboxEnvelopeConsumers.FailedAt} = @FailedAt
                                                                WHERE {Tables.OutboxEnvelopeConsumers.StreamId} = @StreamId
                                                                AND {Tables.OutboxEnvelopeConsumers.Version} = @Version
                                                                AND {Tables.OutboxEnvelopeConsumers.ConsumerKey} = @ConsumerKey
                                                            """;
}