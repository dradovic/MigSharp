using System.Globalization;

using NUnit.Framework;

namespace MigSharp.Oracle.NUnit
{
    [TestFixture, Category("Oracle")]
    public class OracleIntegrationTests : OracleOdbcIntegrationTests
    {
        protected override string ConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "USER ID={0};PASSWORD={1};DATA SOURCE={2}", User, Password, Server); } }

        protected override string ProviderName { get { return ProviderNames.Oracle; } }
    }
}