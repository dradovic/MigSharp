using System.Data.Common;

namespace MigSharp.Versioning
{
    internal interface IDbVersion
    {
        bool Includes(IMigrationMetaData migration);

        /// <summary>
        /// Updates the version to include <paramref name="migration"/>.
        /// </summary>
        /// <param name="connection">An open connection to the database containing the version information.</param>
        /// <param name="migration">The migration that should be included in the version information.</param>
        void Update(DbConnection connection, IMigration migration);
    }
}