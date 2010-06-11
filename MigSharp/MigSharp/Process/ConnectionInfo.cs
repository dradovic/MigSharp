namespace MigSharp.Process
{
    internal class ConnectionInfo
    {
        private readonly string _connectionString;
        private readonly string _providerInvariantName;

        public string ConnectionString { get { return _connectionString; } }
        public string ProviderInvariantName { get { return _providerInvariantName; } }

        public ConnectionInfo(string connectionString, string providerInvariantName)
        {
            _connectionString = connectionString;
            _providerInvariantName = providerInvariantName;
        }
    }
}