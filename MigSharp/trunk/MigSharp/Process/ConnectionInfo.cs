namespace MigSharp.Process
{
    internal class ConnectionInfo
    {
        private readonly string _connectionString;
        private readonly string _providerInvariantName;
        private readonly bool _supportsTransactions;

        public string ConnectionString { get { return _connectionString; } }
        public string ProviderInvariantName { get { return _providerInvariantName; } }
        public bool SupportsTransactions { get { return _supportsTransactions; } }

        public ConnectionInfo(string connectionString, string providerInvariantName, bool supportsTransactions)
        {
            _connectionString = connectionString;
            _providerInvariantName = providerInvariantName;
            _supportsTransactions = supportsTransactions;
        }
    }
}