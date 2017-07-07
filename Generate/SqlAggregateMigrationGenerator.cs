using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp.Generate
{
    internal class SqlAggregateMigrationGenerator : SqlMigrationGeneratorBase
    {
        private readonly History _history;

        protected override string ClassName { get { return "Migration" + GetLatestTimestamp(); } }
        protected override string ExportAttribute { get { return "AggregateMigrationExport"; } }

        public SqlAggregateMigrationGenerator(Server server, Database database, GeneratorOptions options) 
            : base(server, database, options)
        {
            var providerLocator = new ProviderLocator(new ProviderRegistry()); // CLEAN: use DI container
            ProviderInfo provider = providerLocator.GetExactly(DbPlatform.SqlServer2008);
            var versioningTableName = new TableName(options.VersioningTableName, options.VersioningTableSchema);
            _history = new History(versioningTableName, provider.Metadata);
            IDbConnection connection = server.ConnectionContext.SqlConnectionObject;
            connection.Open();
            connection.ChangeDatabase(Database.Name); // ATTENTION: possibly has side-effects
            try
            {
                _history.Load(connection, null);
            }
            finally
            {
                connection.Close();
            }
        }

        private long GetLatestTimestamp()
        {
            IEnumerable<IMigrationMetadata> migrations = _history.GetMigrations()
                .Where(m => m.ModuleName == Options.ModuleName);
            if (!migrations.Any())
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "No migrations found for module '{0}'.", Options.ModuleName));
            }
            return migrations
                .Max(m => m.Timestamp);
        }
    }
}