using System.Data;
using DandyEventStore.Persistence.Sql.Connections;

namespace DandyEventStore.Persistence.Sql;

internal sealed class UnitOfWorkContext : IReadOnlyUnitOfWorkContext, IDisposable
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private bool _disposed;
    
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
        if (_disposed)
            return;

        if (!Completed)
            Transaction.Rollback();

        Transaction.Dispose();
        Connection.Dispose();
        _disposed = true;
    }

    internal void Commit(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Transaction.Commit();
            Completed = true;
        }
        catch
        {
            try
            {
                Transaction.Rollback();
            }
            finally
            {
                Transaction.Dispose();
                Connection.Dispose();
                _disposed = true;
            }

            throw;
        }

        Transaction.Dispose();
        Connection.Dispose();

        Connection = _dbConnectionFactory.CreateAndOpen();
        Transaction = Connection.BeginTransaction();
        Completed = false;
    }
}