namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SqlServer2005, "System.Data.SqlClient", MaximumDbObjectNameLength = MaximumDbObjectNameLength, PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class SqlServer2005Provider : SqlServerProvider
    {
    }
}