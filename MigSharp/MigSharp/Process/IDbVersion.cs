using System.Data;

namespace MigSharp.Process
{
    /// <summary>
    /// Represents the version of a database containing all information about past migrations.
    /// </summary>
    public interface IDbVersion
    {
        /// <summary>
        /// Verifies if a specific migration was performed.
        /// </summary>
        bool Includes(IMigrationMetadata migration);

        /// <summary>
        /// Updates the version to include the migration having the specified <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The metadata of the migration to be included in the versioning table.</param>
        /// <param name="connection">An open connection to the database containing the versioning table.</param>
        /// <param name="transaction">The associated transaction.</param>
        /// <param name="direction">The direction of the migration.</param>
        void Update(IMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction, MigrationDirection direction);
    }
}