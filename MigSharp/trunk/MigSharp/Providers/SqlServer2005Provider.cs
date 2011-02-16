namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SqlServer2005, "System.Data.SqlClient", MaximumDbObjectNameLength = MaximumDbObjectNameLength)]
    internal class SqlServer2005Provider : SqlServerProvider
    {
    }
}