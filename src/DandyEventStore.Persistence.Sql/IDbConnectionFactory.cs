using System.Data;

namespace DandyEventStore.Persistence.Sql;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}