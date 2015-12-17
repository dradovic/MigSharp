namespace MigSharp.Providers
{
    [ProviderExport(Platform.SqlServer, 11, "System.Data.SqlClient", MaximumDbObjectNameLength = MaximumDbObjectNameLength, PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class SqlServer2012Provider : SqlServer2008Provider
    {
    }
}