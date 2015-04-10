using System;
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
                migration += string.Format("db.CreateTable(\"{0}\")", table.Name) + Environment.NewLine;
                foreach (Column column in table.Columns)
                {
                    migration += string.Format("\t.With{0}{1}NullableColumn(\"{2}\")",
                        column.InPrimaryKey ? "PrimaryKey" : string.Empty,
                        column.Nullable ? string.Empty : "Not",
                        column.Name) + Environment.NewLine;
                }
            }
            return migration;
        }
    }
}