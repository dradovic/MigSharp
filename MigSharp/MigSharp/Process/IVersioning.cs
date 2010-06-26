using System.Data;

namespace MigSharp.Process
{
    /// <summary>
    /// Represents the version of a database containing all information about past migrations.
    /// </summary>
    public interface IVersioning
    {
        /// <summary>
        /// Verifies if a specific migration was executed.
        /// </summary>
        bool IsContained(IMigrationMetadata metadata);

        /// <summary>
        /// Updates the versioning to include or exclude the migration depending on the provided <paramref name="direction"/>.
        /// <para>
        /// Post-condition: if <paramref name="direction"/> was <see cref="MigrationDirection.Up"/>, <see cref="IsContained"/> must return <code>true</code>.
        /// Otherwise <paramref name="direction"/> was <see cref="MigrationDirection.Down"/> and <see cref="IsContained"/> must return <code>false</code>.
        /// </para>
        /// </summary>
        /// <param name="metadata">The metadata of the migration to be included in the versioning table.</param>
        /// <param name="connection">An open connection to the database containing the versioning table.</param>
        /// <param name="transaction">The associated transaction.</param>
        /// <param name="direction">The direction of the migration.</param>
        void Update(IMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction, MigrationDirection direction);
    }
}