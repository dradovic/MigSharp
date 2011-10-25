using System.Globalization;

using NUnit.Framework;

namespace MigSharp.Teradata.NUnit
{
    [TestFixture, Category("Teradata")]
    public class TeradataIntegrationTests : TeradataOdbcIntegrationTests
    {
        protected override string ConnectionString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Database={0};User Id={1};Data Source='{2}';Password={3};Session Mode=ANSI",
                    DatabaseName, User, Server, Password);
            }
        }

        protected override string ProviderName { get { return ProviderNames.Teradata; } }
    }
}