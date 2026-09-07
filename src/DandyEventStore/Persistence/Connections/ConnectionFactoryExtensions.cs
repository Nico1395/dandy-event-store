using System.Data;

namespace DandyEventStore.Persistence.Connections;

public static class ConnectionFactoryExtensions
{
    public static IDbConnection CreateAndOpen(this IConnectionFactory factory)
    {
        var connection = factory.Create();

        connection.Open();

        return connection;
    }
}