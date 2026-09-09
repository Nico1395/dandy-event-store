using DandyEventStore.Persistence.Sql.Constants;
using FluentMigrator;

namespace DandyEventStore.Persistence.Migrations;

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
    }

    public override void Down()
    {
        Delete.Table(Tables.Snapshots.Table);
        Delete.Table(Tables.Envelopes.Table);
    }
}