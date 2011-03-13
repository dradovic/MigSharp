using System;
using System.Data;
using System.Data.SQLite;
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
            throw new NotImplementedException();
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