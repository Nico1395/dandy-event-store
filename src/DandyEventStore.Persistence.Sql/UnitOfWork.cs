namespace DandyEventStore.Persistence.Sql;

internal sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private UnitOfWorkContext _context;

    public UnitOfWork(
        SqlStrings sqlStrings,
        IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _context = new UnitOfWorkContext(dbConnectionFactory.CreateAndOpen());

        Envelopes = new EnvelopeRepository(sqlStrings, _context);
        Snapshots = new SnapshotRepository(sqlStrings, _context);
        Outbox = new OutboxRepository(sqlStrings, _context);
    }

    public IEnvelopeRepository Envelopes { get; }
    public ISnapshotRepository Snapshots { get; }
    public IOutboxRepository Outbox { get; }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _context.Transaction.Commit();
            _context.Completed = true;
            _context.Dispose();
        }
        catch
        {
            _context.Transaction.Rollback();
        }
        finally
        {
            var connection = _dbConnectionFactory.CreateAndOpen();
            _context = new UnitOfWorkContext(connection);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}