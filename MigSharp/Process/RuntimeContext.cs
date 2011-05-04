using System.Data;

namespace MigSharp.Process
{
    internal class RuntimeContext : MigrationContext, IRuntimeContext
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public IDbConnection Connection { get { return _connection; } }
        public IDbTransaction Transaction { get { return _transaction; } }

        public RuntimeContext(IDbConnection connection, IDbTransaction transaction, IProviderMetadata providerMetadata)
            : base(providerMetadata)
        {
            _connection = connection;
            _transaction = transaction;
        }
    }
}