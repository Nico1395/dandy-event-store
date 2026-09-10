using System.Data;

namespace DandyEventStore.Persistence.Sql;

internal sealed class UnitOfWorkContext : IReadOnlyUnitOfWorkContext, IDisposable
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public UnitOfWorkContext(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;

        Connection = dbConnectionFactory.CreateAndOpen();
        Transaction = Connection.BeginTransaction();
    }

    public IDbConnection Connection { get; private set; }
    public IDbTransaction Transaction { get; private set; }
    public bool Completed { get; private set; }

    public void Dispose()
    {
        if (!Completed)
            Transaction.Rollback();

        Transaction.Dispose();
        Connection.Dispose();
    }

    internal void Commit(CancellationToken cancellationToken)
    {
        try
        {
            Transaction.Commit();
            Completed = true;
        }
        catch
        {
            Transaction.Rollback();
        }
        finally
        {
            Connection = _dbConnectionFactory.CreateAndOpen();
            Transaction = Connection.BeginTransaction();
            Completed = false;
        }
    }
}