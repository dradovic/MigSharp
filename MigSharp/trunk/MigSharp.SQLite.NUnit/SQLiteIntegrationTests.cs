using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

using MigSharp.NUnit.Integration;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
    [TestFixture, Category("SqlServerCe4")]
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
            // Registering the private deployed SQL CE provider
            // http://stackoverflow.com/questions/1117683/add-a-dbproviderfactory-without-an-app-config

            DataSet dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet == null) throw new InvalidOperationException("cannot configure privately deployed SQL CE 4.0 driver");

            DataRow[] dataRows = dataSet.Tables[0].Select(string.Format(CultureInfo.InvariantCulture, "InvariantName='{0}'", SQLiteProvider.InvariantName));
            foreach (var dataRow in dataRows)
            {
                dataSet.Tables[0].Rows.Remove(dataRow);
            }

            dataSet.Tables[0].Rows.Add(
                "Microsoft SQL Server Compact Data Provider 4.0",
                ".NET Framework Data Provider for Microsoft SQL Server Compact",
                SQLiteProvider.InvariantName,
                "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe");
        }
    }
}