using System;
using System.Data;

namespace MigSharp
{
    /// <summary>
    /// Represents a database.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Gets the context of the migration.
        /// </summary>
        IMigrationContext Context { get; }

        /// <summary>
        /// Gets existing tables.
        /// </summary>
        IExistingTableCollection Tables { get; }

        /// <summary>
        /// Creates a new table on the database.
        /// </summary>
        /// <param name="tableName">The name of the new table.</param>
        /// <param name="primaryKeyConstraintName">Optionally, the name of the primary key constraint.</param>
        ICreatedTable CreateTable(string tableName, string primaryKeyConstraintName);

        /// <summary>
        /// Executes a custom query.
        /// </summary>
        /// <param name="query">Custom SQL which must be understood by all providers that should be supported by this migration.</param>
        void Execute(string query);

        /// <summary>
        /// Executes a custom action against the <see cref="IMigrationContext"/>. Use this method if you need to directly access the
        /// underlying <see cref="IDbConnection"/> or <see cref="IDbTransaction"/>.
        /// </summary>
        void Execute(Action<IRuntimeContext> action);
    }

    /// <summary>
    /// Contains the extension methods for the <see cref="IDatabase"/> interface.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Creates a new table on the database with a default primary key constraint name.
        /// </summary>
        public static ICreatedTable CreateTable(this IDatabase database, string tableName)
        {
            return database.CreateTable(tableName, null);
        }
    }
}