namespace MigSharp.Providers
{
    [ProviderExport(Platform.Oracle, 10, Driver.Odbc, "System.Data.Odbc", SupportsTransactions = false, ParameterExpression = "?", PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class OracleOdbcProvider : OracleProvider
    {
    }
}