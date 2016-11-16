using System.Collections.Generic;

namespace MigSharp
{
    /// <summary>
    /// Represents all information about a migration step scheduled for execution.
    /// </summary>
    public interface IMigrationStepMetadata
    {
        /// <summary>
        /// Gets the module name of the migration.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Gets the direction of the migration execution.
        /// </summary>
        MigrationDirection Direction { get; }

        /// <summary>
        /// Indicates if the module name should be used as the default schema if no other schema name was specified for a command. SQL Server only.
        /// </summary>
        bool UseModuleNameAsDefaultSchema { get; }

        /// <summary>
        /// Gets the metadata of the migrations contained in this step. 
        /// </summary>
        IEnumerable<IMigrationMetadata> Migrations { get; }
    }
}