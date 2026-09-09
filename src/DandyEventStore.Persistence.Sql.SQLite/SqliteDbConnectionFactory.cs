using System.Data;
using Microsoft.Data.Sqlite;

namespace DandyEventStore.Persistence.Sql.SQLite;

internal sealed class SqliteDbConnectionFactory(SqliteConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        return new SqliteConnection(configuration.ConnectionString);
    }
}