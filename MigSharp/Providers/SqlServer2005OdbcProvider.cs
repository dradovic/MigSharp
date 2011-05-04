namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SqlServer2005Odbc, "System.Data.Odbc", SupportsTransactions = true, ParameterExpression = "?")]
    internal class SqlServer2005OdbcProvider : SqlServer2005Provider
    {
    }
}