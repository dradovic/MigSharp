namespace MigSharp
{
    /// <summary>
    /// Used by MEF to represent the metadata of <see cref="MigrationTimestampProviderExportAttribute" />.
    /// </summary>
    public interface IMigrationTimestampProviderExportMetadata
    {
        /// <summary>
        /// Get the module name for the timestamp provider.
        /// </summary>
        string ModuleName { get; }
    }
}