using DandyEventStore.Persistence.Sql.Connections;
using DandyEventStore.Persistence.Sql.Repositories;

namespace DandyEventStore.Persistence.Sql;

internal sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly UnitOfWorkContext _context;

    public UnitOfWork(
        SqlStrings sqlStrings,
        IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _context = new UnitOfWorkContext(dbConnectionFactory);

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

        _context.Commit(cancellationToken);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}