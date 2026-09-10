using System.Data;
using DandyEventStore.Persistence.Sql.Connections;
using Npgsql;

namespace DandyEventStore.Persistence.Sql.Npgsql;

internal sealed class NpgsqlDbConnectionFactory(NpgsqlConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        return new NpgsqlConnection(configuration.ConnectionString);
    }
}