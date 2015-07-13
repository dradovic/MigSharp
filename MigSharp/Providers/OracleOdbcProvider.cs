namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.OracleOdbc, "System.Data.Odbc", SupportsTransactions = false, ParameterExpression = "?", PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class OracleOdbcProvider : OracleProvider
    {
    }
}