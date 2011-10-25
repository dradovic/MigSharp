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
    }
}