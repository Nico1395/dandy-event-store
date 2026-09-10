using DandyEventStore.Persistence.Sql.Constants;
using FluentMigrator;

namespace DandyEventStore.Persistence.Sql.SQLite.Migrations;

[Migration(20260907205400, "Creating tables")]
public class Migration_20260907205400 : Migration
{
    public override void Up()
    {
        Create.Table(Tables.Envelopes.Table)
            .WithColumn(Tables.Envelopes.StreamId).AsString(255).NotNullable().PrimaryKey()
            .WithColumn(Tables.Envelopes.Version).AsInt64().NotNullable().PrimaryKey()
            .WithColumn(Tables.Envelopes.Payload).AsString(int.MaxValue).NotNullable()
            .WithColumn(Tables.Envelopes.Timestamp).AsDateTime().NotNullable()
            .WithColumn(Tables.Envelopes.EventKey).AsString(255).NotNullable();

        Create.Table(Tables.Snapshots.Table)
            .WithColumn(Tables.Snapshots.StreamId).AsString(255).NotNullable().PrimaryKey()
            .WithColumn(Tables.Snapshots.Version).AsInt64().NotNullable().PrimaryKey()
            .WithColumn(Tables.Snapshots.Payload).AsString(int.MaxValue).NotNullable()
            .WithColumn(Tables.Snapshots.Timestamp).AsDateTime().NotNullable()
            .WithColumn(Tables.Snapshots.AggregateKey).AsString(255).NotNullable();

        Create.Table(Tables.OutboxEnvelopes.Table)
            .WithColumn(Tables.OutboxEnvelopes.StreamId).AsString(255).NotNullable().PrimaryKey()
            .WithColumn(Tables.OutboxEnvelopes.Version).AsInt64().NotNullable().PrimaryKey()
            .WithColumn(Tables.OutboxEnvelopes.Payload).AsString(int.MaxValue).NotNullable()
            .WithColumn(Tables.OutboxEnvelopes.Timestamp).AsDateTime().NotNullable()
            .WithColumn(Tables.OutboxEnvelopes.EventKey).AsString(255).NotNullable();

        // SQLite doesnt support the foreign key syntax of FluentMigrator
        Execute.Sql($"""
                     CREATE TABLE "{Tables.OutboxEnvelopeConsumers.Table}" (
                         "{Tables.OutboxEnvelopeConsumers.StreamId}" TEXT NOT NULL,
                         "{Tables.OutboxEnvelopeConsumers.Version}" INTEGER NOT NULL,
                         "{Tables.OutboxEnvelopeConsumers.ConsumerKey}" TEXT NOT NULL,
                         "{Tables.OutboxEnvelopeConsumers.Type}" INTEGER NOT NULL,
                         "{Tables.OutboxEnvelopeConsumers.ConsumedAt}" DATETIME NULL,
                         "{Tables.OutboxEnvelopeConsumers.FailedAt}" DATETIME NULL,

                         PRIMARY KEY (
                             "{Tables.OutboxEnvelopeConsumers.StreamId}",
                             "{Tables.OutboxEnvelopeConsumers.Version}",
                             "{Tables.OutboxEnvelopeConsumers.ConsumerKey}"
                         ),

                         FOREIGN KEY (
                             "{Tables.OutboxEnvelopeConsumers.StreamId}",
                             "{Tables.OutboxEnvelopeConsumers.Version}"
                         )
                         REFERENCES "{Tables.OutboxEnvelopes.Table}" (
                             "{Tables.OutboxEnvelopes.StreamId}",
                             "{Tables.OutboxEnvelopes.Version}"
                         )
                     );
                     """);
    }

    public override void Down()
    {
        Delete.Table(Tables.OutboxEnvelopeConsumers.Table);
        Delete.Table(Tables.OutboxEnvelopes.Table);
        Delete.Table(Tables.Snapshots.Table);
        Delete.Table(Tables.Envelopes.Table);
    }
}