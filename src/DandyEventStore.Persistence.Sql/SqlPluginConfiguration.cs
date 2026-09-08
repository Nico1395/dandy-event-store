using DandyEventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Persistence.Sql;

public abstract class SqlPluginConfiguration : PluginConfiguration
{
    public string? ConnectionString { get; set; }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddFluentMigratorCore();

        services.AddSingleton<IEventRepository, SqlEventRepository>();
        services.AddSingleton<ISnapshotRepository, SqlSnapshotRepository>();
    }
}