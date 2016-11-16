using JetBrains.Annotations;

namespace MigSharp.Process
{
    internal interface IMigrationStep : IMigrationReporter
    {
        IMigrationStepMetadata Metadata { get; }

        /// <summary>
        /// Executes the migration step and updates the versioning information in one transaction.
        /// </summary>
        void Execute(IRuntimeConfiguration configuration, [CanBeNull /* for bootstrap step */] IVersioning versioning);
    }
}