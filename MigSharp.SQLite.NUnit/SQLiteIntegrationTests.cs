using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
// ReSharper disable InconsistentNaming
    [TestFixture, Category("SQLite")]
    public class SQLiteIntegrationTests : SQLiteIntegrationTestsBase
// ReSharper restore InconsistentNaming
    {
        private string _dataFile;

        protected override DbDataAdapter GetDataAdapter(string tableName, string schemaName, out DbCommandBuilder builder)
        {
            var adapter = new SQLiteDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\"", tableName), ConnectionString);
            builder = new SQLiteCommandBuilder(adapter);
            return adapter;
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

        public override void Setup()
        {
            base.Setup();

            _dataFile = Path.GetTempFileName();
            File.Delete(_dataFile);

            SQLiteConnection.CreateFile(_dataFile);
        }

        public override void Teardown()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers(); // see: http://stackoverflow.com/a/24501130/331281
            File.Delete(_dataFile);

            base.Teardown();
        }
    }
}