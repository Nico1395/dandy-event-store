using System.Data;
using Npgsql;

namespace DandyEventStore.Persistence.Sql.Npgsql;

internal sealed class NpgsqlDbConnectionFactory(NpgsqlPluginConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        return new NpgsqlConnection(configuration.ConnectionString);
    }
}