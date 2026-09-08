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
    }

    public override void Down()
    {
        Delete.Table(Tables.Snapshots.Table);
        Delete.Table(Tables.Envelopes.Table);
        Delete.Schema(Sql.Constants.Schema.Name);
    }
}