using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
    [TestFixture, Category("SQLite")]
    public class SQLiteIntegrationTests : SQLiteIntegrationTestsBase
    {
        private string _dataFile;

        protected override DbDataAdapter GetDataAdapter(string tableName, out DbCommandBuilder builder)
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
            File.Delete(_dataFile);

            base.Teardown();
        }
    }
}