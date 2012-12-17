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

        private Database _database;

        public override void Setup()
        {
            base.Setup();

            var server = new Server(Server);

            var database = server.Databases[TestDbName];
            if (database != null)
            {
                database.Drop();
            }

            _database = new Database(server, TestDbName);
            _database.Create();
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, out DbCommandBuilder builder)
        {
            var adapter = new SqlDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}]", tableName), ConnectionString);
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

        public override void Teardown()
        {
            SqlConnection.ClearAllPools();
            _database.Drop();

            base.Teardown();
        }
    }
}