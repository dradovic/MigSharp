using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
    [TestFixture, Category("SQLite")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This is a unit test.")]
    public class InMemoryIntegrationTests : SQLiteIntegrationTestsBase
    {
        private SQLiteConnection _connection;

        protected override DbDataAdapter GetDataAdapter(string tableName, out DbCommandBuilder builder)
        {
            var adapter = new SQLiteDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\"", tableName), _connection);
            builder = new SQLiteCommandBuilder(adapter);
            return adapter;
        }

        protected override string ConnectionString
        {
            get
            {
                var builder = new SQLiteConnectionStringBuilder
                {
                    DataSource = ":memory:"
                };
                return builder.ConnectionString;
            }
        }

        protected override System.Data.IDbConnection CustomConnection { get { return _connection; } }

        public override void Setup()
        {
            base.Setup();
            _connection = new SQLiteConnection(ConnectionString);
            _connection.Open();
        }

        public override void Teardown()
        {
            _connection.Close();

            base.Teardown();
        }
    }
}