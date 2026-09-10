using System.Data;

namespace DandyEventStore.Persistence.Sql;

public interface IReadOnlyUnitOfWorkContext
{
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }
    bool Completed { get; }
}