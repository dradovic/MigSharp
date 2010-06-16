using System.Data;

namespace MigSharp.Process
{
    internal interface IDbVersion
    {
        bool Includes(IMigrationMetaData migration);

        /// <summary>
        /// Updates the version to include the migration having the specified <paramref name="metaData"/>.
        /// </summary>
        /// <param name="connection">An open connection to the database containing the version information.</param>
        /// <param name="metaData">The metadata of the migration to be included in the version information.</param>
        void Update(IDbConnection connection, IMigrationMetaData metaData);
    }
}