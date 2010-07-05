using System.Data;

namespace MigSharp.Process
{
    internal class MigrationContext : IMigrationContext
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public IDbConnection Connection { get { return _connection; } }
        public IDbTransaction Transaction { get { return _transaction; } }

        public MigrationContext(IDbConnection connection, IDbTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }
    }
}