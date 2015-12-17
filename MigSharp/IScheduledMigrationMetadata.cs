namespace MigSharp
{
    /// <summary>
    /// Represents all descriptive information about a migration scheduled for execution.
    /// </summary>
    public interface IScheduledMigrationMetadata : IMigrationMetadata
    {
        /// <summary>
        /// Gets the direction of the migration execution.
        /// </summary>
        MigrationDirection Direction { get; }

        /// <summary>
        /// Indicates if the module name should be used as the default schema if no other schema name was specified for a command. SQL Server only.
        /// </summary>
        bool UseModuleNameAsDefaultSchema { get; }
    }
}