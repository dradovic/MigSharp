using System.Data;

namespace MigSharp.Process
{
    internal class RuntimeContext : MigrationContext, IRuntimeContext
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly IDbCommandExecutor _executor;

        public IDbConnection Connection { get { return _connection; } }
        public IDbTransaction Transaction { get { return _transaction; } }
        public IDbCommandExecutor CommandExecutor { get { return _executor; } }

        public RuntimeContext(IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor executor, IProviderMetadata providerMetadata)
            : base(providerMetadata)
        {
            _connection = connection;
            _transaction = transaction;
            _executor = executor;
        }
    }
}