using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;

using MigSharp.NUnit.Integration;

using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    [TestFixture, Category("SqlServerCe3.5")]
    public class SqlServerCe35IntegrationTests : IntegrationTestsBase
    {
        private string _dataFile;

        protected override string ProviderName { get { return ProviderNames.SqlServerCe35; } }

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
                return "Data Source=" + _dataFile;
            }
        }

        public override void Teardown()
        {
            File.Delete(_dataFile);

            base.Teardown();
        }
    }
}