using System.Data;
using DandyEventStore.Persistence;

namespace DandyEventStore.Persistence.Sql;

internal sealed class SqlTransactionFactory(
    SqlStrings sqlStrings,
    IDbConnectionFactory dbConnectionFactory) : ITransactionFactory
{
    public ITransaction Create()
    {
        return new SqlTransaction(sqlStrings, dbConnectionFactory);
    }

    private sealed class SqlTransaction : ITransaction
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private bool _completed;

        public SqlTransaction(SqlStrings sqlStrings, IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateAndOpen();
            _transaction = _connection.BeginTransaction();
            EventRepository = new SqlEventRepository(sqlStrings, _connection, _transaction);
            OutboxEnvelopeRepository = new SqlOutboxEnvelopeRepository(sqlStrings, _connection, _transaction);
        }

        public IEventRepository EventRepository { get; }
        public IOutboxEnvelopeRepository OutboxEnvelopeRepository { get; }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _transaction.Commit();
            _completed = true;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            if (!_completed)
                _transaction.Rollback();

            _transaction.Dispose();
            _connection.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
