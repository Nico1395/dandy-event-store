using DandyEventStore.Configuration;

namespace DandyEventStore.Persistence.Sql.Npgsql;

public static class EventStoreConfigurationBuilderExtensions
{
    public static EventStoreConfigurationBuilder UseNpgsql(this EventStoreConfigurationBuilder builder, Action<NpgsqlConfigurationBuilder>? builderAction = null)
    {
        var configurationBuilder = new NpgsqlConfigurationBuilder();
        builderAction?.Invoke(configurationBuilder);
        var configuration = configurationBuilder.Build();

        return builder.UsePlugin(configuration);
    }
}