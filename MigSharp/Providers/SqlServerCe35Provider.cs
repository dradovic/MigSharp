namespace MigSharp.Providers
{
    /// <summary>
    /// MigSharp provider for Microsoft SQL Compact Edition 3.5.
    /// </summary>
    [ProviderExport(ProviderNames.SqlServerCe35, InvariantName, MaximumDbObjectNameLength = 128)]
    internal class SqlServerCe35Provider : SqlServerCeProviderBase
    {
        private const string InvariantName = "System.Data.SqlServerCe.3.5";
    }
}