using System.Data;

namespace MigSharp.Process
{
    internal interface IDbVersion
    {
        bool Includes(IMigrationMetadata migration);

        /// <summary>
        /// Updates the version to include the migration having the specified <paramref name="metadata"/>.
        /// </summary>
        /// <param name="connection">An open connection to the database containing the versioning table.</param>
        /// <param name="metadata">The metadata of the migration to be included in the versioning table.</param>
        /// <param name="transaction"></param>
        void Update(IMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction);
    }
}