using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Persistence.Sql.SQLite;

internal sealed class SqliteConfiguration : SqlPluginConfiguration
{
    private static readonly Assembly[]? _assemblies = [typeof(SqliteConfiguration).Assembly];

    public override string Slot => "persistence";

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.ConfigureRunner(runner =>
        {
            runner.AddSQLite()
                .WithGlobalConnectionString(ConnectionString)
                .ScanIn(_assemblies).For.Migrations();
        });

        services.AddSingleton<IDbConnectionFactory, SqliteDbConnectionFactory>();
        services.AddSingleton<SqlStrings, SqliteSqlStrings>();
        services.AddSingleton(this);
    }
}