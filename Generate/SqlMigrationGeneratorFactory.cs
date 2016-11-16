using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;

namespace MigSharp.Generate
{
    public class SqlMigrationGeneratorFactory
    {
        private readonly string _connectionString;

        public SqlMigrationGeneratorFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IGenerator Create(GeneratorOptions options)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
            var server = new Server(builder.DataSource);
            var database = server.Databases[builder.InitialCatalog];
            database.Refresh(); // loads the meta-data

            if (database.Tables.Contains(options.VersioningTableName))
            {
                return new SqlAggregateMigrationGenerator(server, database, options);
            }
            else
            {
                return new SqlFirstMigrationGenerator(server, database, options);
            }
        }
    }
}