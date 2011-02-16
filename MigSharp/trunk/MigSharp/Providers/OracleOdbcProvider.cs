using System.Data;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.OracleOdbc, "System.Data.Odbc", SupportsTransactions = false, ParameterExpression = "?")]
    [Supports(DbType.Byte)]
    internal class OracleOdbcProvider : OracleProvider
    {
    }
}