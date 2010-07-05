using System.Data;

namespace MigSharp
{
    public interface IMigrationContext
    {
        /// <summary>
        /// Gets the connection which is used to perform the migration.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Gets the transaction which is used to perform the migration.
        /// </summary>
        IDbTransaction Transaction { get; }
    }
}