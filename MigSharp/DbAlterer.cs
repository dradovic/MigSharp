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
        internal IRuntimeConfiguration Configuration { get; }

        protected DbAlterer(string connectionString, DbPlatform dbPlatform, DbAltererOptions options)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (dbPlatform == null) throw new ArgumentNullException("dbPlatform");
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            Configuration = CreateRuntimeConfiguration(connectionString, dbPlatform, options);
        }

        private static RuntimeConfiguration CreateRuntimeConfiguration(string connectionString, DbPlatform dbPlatform, DbAltererOptions options)
        {
            var providerLocator = new ProviderLocator(new ProviderRegistry()); // CLEAN: use DI container

            var providerInfo = providerLocator.GetLatest(dbPlatform);
            var validatorFactory = new ValidatorFactory(providerInfo, options, providerLocator);
            var validator = validatorFactory.Create();
            var connectionInfo = new ConnectionInfo(connectionString, providerInfo.Metadata.InvariantName, providerInfo.Metadata.SupportsTransactions, providerInfo.Metadata.EnableAnsiQuotesCommand);
            var sqlDispatcher = new SqlDispatcher(options.GetScriptingOptions(), providerInfo.Provider, providerInfo.Metadata);
            return new RuntimeConfiguration(providerInfo, connectionInfo, validator, sqlDispatcher);
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

            Configuration.ConnectionFactory.UseCustomConnection(connection);
        }
    }
}