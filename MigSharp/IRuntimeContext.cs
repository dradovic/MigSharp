using System.Data;

namespace MigSharp
{
    /// <summary>
    /// Represents the run-time context of a migration.
    /// </summary>
    public interface IRuntimeContext : IMigrationContext
    {
        /// <summary>
        /// Gets the connection which is used to perform the migration.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Gets the transaction which is used to perform the migration.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Gets the <see cref="IDbCommandExecutor"/> that should be used to execute database modifying commands.
        /// This ensures that they are logged and scripted consistently.
        /// </summary>
        IDbCommandExecutor CommandExecutor { get; }
    }
}