using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.SqlServer.Management.Smo;
using MigSharp.NUnit.Integration;

namespace MigSharp.SqlServer.NUnit
{
    public abstract class SqlServerIntegrationTests : IntegrationTestsBase
    {
        protected const string Server = "localhost";
        protected const string TestDbName = "MigSharp_TestDb";

        private Server _server;
        private Database _database;

        protected Server ServerSmo { get { return _server; } }

        protected Database DatabaseSmo { get { return _database; } }

        public override void Setup()
        {
            base.Setup();

            _server = new Server(Server);

            var database = _server.Databases[TestDbName];
            if (database != null)
            {
                database.Drop();
            }

            _database = new Database(_server, TestDbName);
            _database.Create();
            var schema = new Schema(_database, CustomVersioningTableSchema);
            schema.Owner = "dbo";
            schema.Create();
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, string schemaName, out DbCommandBuilder builder)
        {
            var adapter = new SqlDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", schemaName ?? "dbo", tableName), ConnectionString);
            builder = new SqlCommandBuilder(adapter);
            return adapter;
        }

        protected override string ConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                    {
                        DataSource = Server,
                        InitialCatalog = TestDbName,
                        IntegratedSecurity = true,
                    };
                return builder.ConnectionString;
            }
        }

        protected override bool ProviderSupportsSchemas { get { return true; } }

        public override void Teardown()
        {
            SqlConnection.ClearAllPools();
            _database.Drop();

            base.Teardown();
        }
    }
}