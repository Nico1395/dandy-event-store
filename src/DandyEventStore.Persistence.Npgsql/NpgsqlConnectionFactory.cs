using System.Data;
using DandyEventStore.Persistence.Configuration;
using DandyEventStore.Persistence.Connections;
using Npgsql;

namespace DandyEventStore.Persistence.Npgsql;

internal sealed class NpgsqlConnectionFactory(PersistenceConfiguration persistenceConfiguration) : IConnectionFactory
{
    public IDbConnection Create()
    {
        return new NpgsqlConnection(persistenceConfiguration.ConnectionString);
    }
}