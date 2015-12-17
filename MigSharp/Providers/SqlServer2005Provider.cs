namespace MigSharp.Providers
{
    [ProviderExport(Platform.SqlServer, 9, "System.Data.SqlClient", MaximumDbObjectNameLength = MaximumDbObjectNameLength, PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class SqlServer2005Provider : SqlServerProvider
    {
    }
}