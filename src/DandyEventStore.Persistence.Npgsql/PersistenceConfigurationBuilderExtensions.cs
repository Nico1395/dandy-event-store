using DandyEventStore.Persistence.Configuration;

namespace DandyEventStore.Persistence.Npgsql;

public static class PersistenceConfigurationBuilderExtensions
{
    public static PersistenceConfigurationBuilder UseNpgsql(this PersistenceConfigurationBuilder builder, string? connectionString)
    {
        builder.SqlStringsType = typeof(NpgsqlSqlStrings);
        builder.DbConnectionFactoryType = typeof(NpgsqlConnectionFactory);
        builder.ConnectionString = connectionString;

        return builder;
    }
}