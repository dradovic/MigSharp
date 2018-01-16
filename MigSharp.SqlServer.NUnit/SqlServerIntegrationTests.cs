using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using Microsoft.SqlServer.Management.Smo;
using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2012")]
    public partial class SqlServerIntegrationTests : IntegrationTestsBase
    {
        protected const string Server = "localhost";
        protected const string TestDbName = "MigSharp_TestDb";

        private Server _server;
        private Database _database;

        protected Server ServerSmo { get { return _server; } }

        protected Database DatabaseSmo { get { return _database; } }

        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServer2012; } }

        public override void Setup()
        {
            base.Setup();

            _server = new Server(Server);

            Database database;
            if (_server.Databases.Contains(TestDbName))
            {
                database = _server.Databases[TestDbName];
                database.Drop();
            }

            database = new Database(_server, TestDbName);
            database.Create();
            _database = _server.Databases[TestDbName]; // reference the created database (see: https://docs.microsoft.com/en-us/sql/relational-databases/server-management-objects-smo/tasks/creating-altering-and-removing-databases)
            Assert.IsNotNull(_database, "There was a problem creating and accessing the database using SMO.");
            var schema = new Schema(_database, CustomVersioningTableSchema);
            schema.Owner = "dbo";
            schema.Create();
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, string schemaName, bool forUpdating)
        {
            var adapter = new SqlDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", schemaName ?? "dbo", tableName), ConnectionString);
            if (forUpdating)
            {
#if NET462
                var builder = new SqlCommandBuilder(adapter);
                adapter.InsertCommand = builder.GetInsertCommand();
#else
                Debug.Assert(tableName == "MigSharp" || tableName == "MyVersioningTable");
                var insertCommand = adapter.SelectCommand.Connection.CreateCommand();
                insertCommand.CommandText = $"INSERT INTO [{schemaName ?? "dbo"}].[{tableName}] VALUES (@p1, @p2, @p3)";
                SqlParameter p1 = insertCommand.Parameters.Add("@p1", SqlDbType.BigInt);
                p1.SourceColumn = "Timestamp";
                p1.SourceVersion = DataRowVersion.Proposed;
                SqlParameter p2 = insertCommand.Parameters.Add("@p2", SqlDbType.VarChar);
                p2.SourceColumn = "Module";
                p2.SourceVersion = DataRowVersion.Proposed;
                SqlParameter p3 = insertCommand.Parameters.Add("@p3", SqlDbType.VarChar);
                p3.SourceColumn = "Tag";
                p3.SourceVersion = DataRowVersion.Proposed;
                adapter.InsertCommand = insertCommand;
#endif
            }
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
            if (_database != null)
            {
                _database.Drop();
            }

            base.Teardown();
        }
    }
}