using System.Data;

namespace DandyEventStore.Persistence.Sql;

public static class DbConnectionFactoryExtensions
{
    public static IDbConnection CreateAndOpen(this IDbConnectionFactory factory)
    {
        var connection = factory.Create();

        connection.Open();

        return connection;
    }
}