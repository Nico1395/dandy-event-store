using System.Data;

namespace DandyEventStore.Persistence.Connections;

public interface IConnectionFactory
{
    IDbConnection Create();
}