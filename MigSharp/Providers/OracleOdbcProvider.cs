namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.OracleOdbc, "System.Data.Odbc", SupportsTransactions = false, ParameterExpression = "?")]
    internal class OracleOdbcProvider : OracleProvider
    {
    }
}