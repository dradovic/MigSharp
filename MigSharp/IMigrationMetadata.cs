namespace MigSharp
{
    /// <summary>
    /// Represents all descriptive information about a migration.
    /// </summary>
    public interface IMigrationMetadata
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
        /// Gets the timestamp of the migration.
        /// </summary>
        long Timestamp { get; }
    }
}