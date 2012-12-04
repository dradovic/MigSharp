namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SqlServer2012, "System.Data.SqlClient", MaximumDbObjectNameLength = MaximumDbObjectNameLength)]
    internal class SqlServer2012Provider : SqlServer2008Provider
    {
    }
}