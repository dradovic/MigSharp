using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

using MigSharp.NUnit.Integration;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
    [TestFixture, Category("SQLite")]
    internal class SQLiteIntegrationTests : IntegrationTestsBase
    {
        private string _dataFile;

        protected override DataTable GetTable(string tableName)
        {
            var table = new DataTable(tableName) { Locale = CultureInfo.InvariantCulture };
            try
            {
                using (var adapter = new SQLiteDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\"", tableName), ConnectionString))
                {
                    adapter.Fill(table);
                }
            }
            catch (SQLiteException)
            {
                table = null;
            }
            return table;
        }

        protected override string ConnectionString
        {
            get
            {
                var builder = new SQLiteConnectionStringBuilder
                {
                    DataSource = _dataFile
                };
                return builder.ConnectionString;
            }
        }
        protected override string ProviderName { get { return ProviderNames.SQLite; } }

        public override void Setup()
        {
            base.Setup();

            _dataFile = Path.GetTempFileName();
            File.Delete(_dataFile);

            SQLiteConnection.CreateFile(_dataFile);
        }

        public override void Teardown()
        {
            File.Delete(_dataFile);

            base.Teardown();
        }

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            RegisterAdoNetProvider(
                "SQLite Data Provider",
                ".Net Framework Data Provider for SQLite",
                SQLiteProvider.InvariantName,
                "System.Data.SQLite.SQLiteFactory, System.Data.SQLite");
        }
    }
}