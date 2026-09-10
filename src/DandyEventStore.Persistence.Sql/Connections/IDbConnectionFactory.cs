using System.Data;

namespace DandyEventStore.Persistence.Sql.Connections;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}