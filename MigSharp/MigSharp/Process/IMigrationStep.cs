namespace MigSharp.Process
{
    internal interface IMigrationStep
    {
        IMigrationMetadata Metadata { get; }

        /// <summary>
        /// Executes the migration step and updates the versioning information in one transaction.
        /// </summary>
        /// <param name="versioning">Might be null in the case of a bootstrap step.</param>
        /// <param name="direction"></param>
        void Execute(IVersioning versioning, MigrationDirection direction);
    }
}