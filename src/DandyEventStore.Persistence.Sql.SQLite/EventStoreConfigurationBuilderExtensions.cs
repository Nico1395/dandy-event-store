using DandyEventStore.Configuration;

namespace DandyEventStore.Persistence.Sql.SQLite;

public static class EventStoreConfigurationBuilderExtensions
{
    public static EventStoreConfigurationBuilder UseSqlite(this EventStoreConfigurationBuilder builder, Action<SqliteConfigurationBuilder>? builderAction = null)
    {
        var configurationBuilder = new SqliteConfigurationBuilder();
        builderAction?.Invoke(configurationBuilder);
        var configuration = configurationBuilder.Build();

        return builder.UsePlugin(configuration);
    }
}