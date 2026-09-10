using System.Data;

namespace DandyEventStore.Persistence.Sql;

internal sealed class UnitOfWorkContext(IDbConnection connection) : IDisposable
{
    public IDbConnection Connection { get; } = connection;
    public IDbTransaction Transaction { get; } = connection.BeginTransaction();
    public bool Completed { get; set; }

    public void Dispose()
    {
        if (!Completed)
            Transaction.Rollback();

        Transaction.Dispose();
        Connection.Dispose();
    }
}