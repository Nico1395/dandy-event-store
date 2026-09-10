using DandyEventStore.Persistence.Sql.Constants;
using FluentMigrator;

namespace DandyEventStore.Persistence.Migrations;

[Migration(20260907191400, "Creating schema and tables")]
public class Migration_20260907191400 : Migration
{
    public override void Up()
    {
        Create.Schema(Sql.Constants.Schema.Name);

        Create.Table(Tables.Envelopes.Table)
            .WithColumn(Tables.Envelopes.StreamId).AsString(255).NotNullable()
            .WithColumn(Tables.Envelopes.Payload).AsString(int.MaxValue).NotNullable()
            .WithColumn(Tables.Envelopes.Version).AsInt64().NotNullable()
            .WithColumn(Tables.Envelopes.Timestamp).AsDateTime().NotNullable()
            .WithColumn(Tables.Envelopes.EventKey).AsString(255).NotNullable();
        Create.PrimaryKey("pk_envelopes")
            .OnTable(Tables.Envelopes.Table)
            .Columns(Tables.Envelopes.StreamId, Tables.Envelopes.Version);

        Create.Table(Tables.Snapshots.Table)
            .WithColumn(Tables.Snapshots.StreamId).AsString(255).NotNullable()
            .WithColumn(Tables.Snapshots.Payload).AsString(int.MaxValue).NotNullable()
            .WithColumn(Tables.Snapshots.Version).AsInt64().NotNullable()
            .WithColumn(Tables.Snapshots.Timestamp).AsDateTime().NotNullable()
            .WithColumn(Tables.Snapshots.AggregateKey).AsString(255).NotNullable();
        Create.PrimaryKey("pk_snapshots")
            .OnTable(Tables.Snapshots.Table)
            .Columns(Tables.Snapshots.StreamId, Tables.Snapshots.Version);

        Create.Table(Tables.OutboxEnvelopes.Table)
            .WithColumn(Tables.OutboxEnvelopes.StreamId).AsString(255).NotNullable()
            .WithColumn(Tables.OutboxEnvelopes.Version).AsInt64().NotNullable()
            .WithColumn(Tables.OutboxEnvelopes.Payload).AsString(int.MaxValue).NotNullable()
            .WithColumn(Tables.OutboxEnvelopes.Timestamp).AsDateTime().NotNullable()
            .WithColumn(Tables.OutboxEnvelopes.EventKey).AsString(255).NotNullable();
        Create.PrimaryKey("pk_outbox_envelopes")
            .OnTable(Tables.OutboxEnvelopes.Table)
            .Columns(Tables.OutboxEnvelopes.StreamId, Tables.OutboxEnvelopes.Version);

        Create.Table(Tables.OutboxEnvelopeConsumers.Table)
            .WithColumn(Tables.OutboxEnvelopeConsumers.StreamId).AsString(255).NotNullable()
            .WithColumn(Tables.OutboxEnvelopeConsumers.Version).AsInt64().NotNullable()
            .WithColumn(Tables.OutboxEnvelopeConsumers.ConsumerKey).AsString(255).NotNullable()
            .WithColumn(Tables.OutboxEnvelopeConsumers.Type).AsInt16().NotNullable()
            .WithColumn(Tables.OutboxEnvelopeConsumers.ConsumedAt).AsDateTime().Nullable()
            .WithColumn(Tables.OutboxEnvelopeConsumers.FailedAt).AsDateTime().Nullable();
        Create.PrimaryKey("pk_outbox_envelope_consumers")
            .OnTable(Tables.OutboxEnvelopeConsumers.Table)
            .Columns(Tables.OutboxEnvelopeConsumers.StreamId, Tables.OutboxEnvelopeConsumers.Version, Tables.OutboxEnvelopeConsumers.ConsumerKey);

        Create.ForeignKey("fk_outbox_envelope_consumers")
            .FromTable(Tables.OutboxEnvelopeConsumers.Table).ForeignColumns(Tables.OutboxEnvelopeConsumers.StreamId, Tables.OutboxEnvelopeConsumers.Version)
            .ToTable(Tables.OutboxEnvelopes.Table).PrimaryColumns(Tables.OutboxEnvelopes.StreamId, Tables.OutboxEnvelopes.Version);
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_outbox_envelope_consumers");
        Delete.Table(Tables.OutboxEnvelopeConsumers.Table);
        Delete.Table(Tables.OutboxEnvelopes.Table);
        Delete.Table(Tables.Snapshots.Table);
        Delete.Table(Tables.Envelopes.Table);
        Delete.Schema(Sql.Constants.Schema.Name);
    }
}