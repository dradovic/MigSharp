namespace MigSharp.Process
{
    /// <summary>
    /// Used by MEF to represent the metadata of a <see cref="AggregateMigrationExportAttribute"/>.
    /// </summary>
    public interface IAggregateMigrationExportMetadata
    {
        /// <summary>
        /// Gets the module name of the aggregate migration.
        /// </summary>
        string ModuleName { get; }
    }
}