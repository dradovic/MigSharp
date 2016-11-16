using System.Collections.Generic;
using System.Data;

namespace MigSharp.Process
{
    /// <summary>
    /// Represents the version of a database containing all information about past migrations.
    /// </summary>
    public interface IVersioning
    {
        /// <summary>
        /// Gets a list of executed migrations.
        /// <para>
        /// The idea is that the object implementing this interface initializes itself once upon construction and builds a cache
        /// of executed migrations.
        /// </para>
        /// </summary>
        IEnumerable<IMigrationMetadata> ExecutedMigrations { get; }

        /// <summary>
        /// Updates the versioning to include or exclude the migration depending on the provided <see cref="MigrationDirection"/> of <paramref name="metadata"/>.
        /// <para>
        /// Post-condition: if <see cref="MigrationDirection"/> of <paramref name="metadata"/> was <see cref="MigrationDirection.Up"/>, <see cref="ExecutedMigrations"/> must contain the migration.
        /// Otherwise the <see cref="MigrationDirection"/> of <paramref name="metadata"/> was <see cref="MigrationDirection.Down"/> and <see cref="ExecutedMigrations"/> must must not contain the migration.
        /// </para>
        /// </summary>
        /// <param name="metadata">The metadata of the migration to be included in the versioning table.</param>
        /// <param name="connection">An open connection to the database containing the versioning table.</param>
        /// <param name="transaction">The associated transaction.</param>
        /// <param name="commandExecutor">Used to execute <see cref="IDbCommand"/>s.</param>
        void Update(IMigrationStepMetadata metadata, IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor commandExecutor);
    }
}