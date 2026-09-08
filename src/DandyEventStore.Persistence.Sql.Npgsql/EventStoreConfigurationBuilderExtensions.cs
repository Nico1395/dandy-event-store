using DandyEventStore.Configuration;

namespace DandyEventStore.Persistence.Sql.Npgsql;

public static class EventStoreConfigurationBuilderExtensions
{
    public static EventStoreConfigurationBuilder UseNpgsql(this EventStoreConfigurationBuilder builder)
    {
        builder.UsePlugin(new NpgsqlPluginConfiguration());
        return builder;
    }
}