using System;
using System.Data;
using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp
{
    /// <summary>
    /// Base class for database schema altering classes.
    /// </summary>
    public abstract class DbAlterer
    {
        private readonly ProviderInfo _provider;
        private readonly ConnectionInfo _connectionInfo;
        private readonly DbConnectionFactory _connectionFactory = new DbConnectionFactory();
        private readonly Validator _validator;

        internal ProviderInfo Provider { get { return _provider; } }
        internal ConnectionInfo ConnectionInfo { get { return _connectionInfo; } }
        internal DbConnectionFactory ConnectionFactory { get { return _connectionFactory; } }
        internal Validator Validator { get { return _validator; } }

        /// <summary>
        /// Initializer.
        /// </summary>
        protected DbAlterer(string connectionString, DbPlatform dbPlatform, DbAltererOptions options)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            var providerLocator = new ProviderLocator(new ProviderFactory()); // CLEAN: use DI container

            _provider = providerLocator.GetLatest(dbPlatform);
            var validatorFactory = new ValidatorFactory(_provider, options, providerLocator);
            _validator = validatorFactory.Create();
            _connectionInfo = new ConnectionInfo(connectionString, _provider.Metadata.InvariantName, _provider.Metadata.SupportsTransactions, _provider.Metadata.EnableAnsiQuotesCommand);
        }

        /// <summary>
        /// <para>Injects an existing connection which is used for all database accesses without opening or closing it. In this case,
        /// the provided ConnectionString will be ignored.</para>
        /// <para>The caller is responsible for opening the connection before executing the migrations and disposing the connection afterwards.</para>
        /// <para>Use this method only if you really have to.</para>
        /// </summary>
        /// <remarks>
        /// SQLite in-memory databases require the connection to be open all the time (see https://github.com/dradovic/MigSharp/pull/38).
        /// </remarks>
        /// <param name="connection">The connection to be used.</param>
        public void UseCustomConnection(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            ConnectionFactory.UseCustomConnection(connection);
        }
    }
}