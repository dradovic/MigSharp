namespace MigSharp.Process
{
    internal interface IMigrationStep : IMigrationReporter
    {
        IScheduledMigrationMetadata Metadata { get; }

        /// <summary>
        /// Executes the migration step and updates the versioning information in one transaction.
        /// </summary>
        /// <param name="versioning">Might be null in the case of a bootstrap step.</param>
        void Execute(IVersioning versioning);
    }
}