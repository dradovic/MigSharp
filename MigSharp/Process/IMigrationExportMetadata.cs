namespace MigSharp.Process
{
    /// <summary>
    /// Used by MEF to represent the metadata of a <see cref="MigrationExportAttribute"/>.
    /// </summary>
    public interface IMigrationExportMetadata
    {
        /// <summary>
        /// Gets the associated tag of the migration.
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Gets the module name of the migration.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Indicates if the module name should be used as the default schema if no other schema name was specified for a command. SQL Server only.
        /// </summary>
        bool UseModuleNameAsDefaultSchema { get; }
    }
}