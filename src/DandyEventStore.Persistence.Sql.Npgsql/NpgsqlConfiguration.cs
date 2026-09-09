using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Persistence.Sql.Npgsql;

internal sealed class NpgsqlConfiguration : SqlPluginConfiguration
{
    private static readonly Assembly[]? _assemblies = [typeof(NpgsqlConfiguration).Assembly];

    public override string Slot => "persistence";

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.ConfigureRunner(runner =>
        {
            runner.AddPostgres()
                .WithGlobalConnectionString(ConnectionString)
                .ScanIn(_assemblies).For.Migrations();
        });

        services.AddSingleton<IDbConnectionFactory, NpgsqlDbConnectionFactory>();
        services.AddSingleton<SqlStrings, NpgsqlSqlStrings>();
        services.AddSingleton(this);
    }
}