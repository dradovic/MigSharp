using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;

using MigSharp.NUnit.Integration;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    [TestFixture, Category("SqlServerCe4")]
    public class SqlCe4ServerIntegrationTests : IntegrationTestsBase
    {
        private string _dataFile;

        protected override string ProviderName { get { return ProviderNames.SqlServerCe4; } }

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            // Registering the private deployed SQL CE provider
            // http://stackoverflow.com/questions/1117683/add-a-dbproviderfactory-without-an-app-config

            DataSet dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet == null) throw new InvalidOperationException("cannot configure privately deployed SQL CE 4.0 driver");

            DataRow[] dataRows = dataSet.Tables[0].Select(string.Format(CultureInfo.InvariantCulture, "InvariantName='{0}'", SqlServerCe4Provider.SqlCe4InvariantName));
            foreach (var dataRow in dataRows)
            {
                dataSet.Tables[0].Rows.Remove(dataRow);
            }

            dataSet.Tables[0].Rows.Add(
                "Microsoft SQL Server Compact Data Provider 4.0",
                ".NET Framework Data Provider for Microsoft SQL Server Compact",
                SqlServerCe4Provider.SqlCe4InvariantName,
                "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe");
        }

        public override void Setup()
        {
            base.Setup();

            _dataFile = Path.GetTempFileName();
            File.Delete(_dataFile);

            using (var engine = new SqlCeEngine(ConnectionString))
            {
                engine.CreateDatabase();
            }
        }

        protected override DataTable GetTable(string tableName)
        {
            var table = new DataTable(tableName) { Locale = CultureInfo.InvariantCulture };
            try
            {
                using (var adapter = new SqlCeDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\"", tableName), ConnectionString))
                {
                    adapter.Fill(table);
                }
            }
            catch (SqlCeException)
            {
                table = null;
            }

            return table;
        }

        protected override string ConnectionString
        {
            get
            {
                var builder = new SqlCeConnectionStringBuilder
                {
                    DataSource = _dataFile
                };
                return builder.ConnectionString;
            }
        }

        public override void Teardown()
        {
            File.Delete(_dataFile);

            base.Teardown();
        }
    }
}