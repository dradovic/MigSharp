using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;

namespace MigSharp.Generate
{
    internal class SqlMigrationGenerator
    {
        private readonly string _connectionString;

        public SqlMigrationGenerator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string Generate()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
            var server = new Server(builder.DataSource);
            Database database = server.Databases[builder.InitialCatalog];
            database.Refresh(); // load the meta-data
            string migration = string.Empty;
            foreach (Table table in database.Tables)
            {
                migration += string.Format("db.CreateTable(\"{0}\")", table.Name);
            }
            return migration;
        }
    }
}