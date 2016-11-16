using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp
{
    internal class RuntimeConfiguration : IRuntimeConfiguration
    {
        private readonly ProviderInfo _providerInfo;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IDbConnectionFactory _connectionFactory = new DbConnectionFactory();
        private readonly IValidator _validator;
        private readonly ISqlDispatcher _sqlDispatcher;

        public ProviderInfo ProviderInfo { get { return _providerInfo; } }
        public ConnectionInfo ConnectionInfo { get { return _connectionInfo; } }
        public IDbConnectionFactory ConnectionFactory { get { return _connectionFactory; } }
        public IValidator Validator { get { return _validator; } }
        public ISqlDispatcher SqlDispatcher { get { return _sqlDispatcher; } }

        public RuntimeConfiguration(ProviderInfo providerInfo, ConnectionInfo connectionInfo, IValidator validator, ISqlDispatcher sqlDispatcher)
        {
            _providerInfo = providerInfo;
            _connectionInfo = connectionInfo;
            _validator = validator;
            _sqlDispatcher = sqlDispatcher;
        }
    }
}