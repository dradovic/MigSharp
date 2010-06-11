using System.Data;

namespace MigSharp.Process
{
    internal interface IDbVersion
    {
        bool Includes(IMigrationMetaData migration);

        /// <summary>
        /// Updates the version to include <paramref name="migration"/>.
        /// </summary>
        /// <param name="connection">An open connection to the database containing the version information.</param>
        /// <param name="migration">The migration that should be included in the version information.</param>
        void Update(IDbConnection connection, IMigration migration);
    }
}