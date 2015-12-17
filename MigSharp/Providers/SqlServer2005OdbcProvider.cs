namespace MigSharp.Providers
{
    [ProviderExport(Platform.SqlServer, 9, Driver.Odbc, "System.Data.Odbc", SupportsTransactions = true, ParameterExpression = "?", PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class SqlServer2005OdbcProvider : SqlServer2005Provider
    {
    }
}