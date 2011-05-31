using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a type that knows how to provide database-specific DDL statements.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Checks whether a user created table exists on the database. The returned SQL command must yield 0 if the table does not exist.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>The SQL command to be executed.</returns>
        string ExistsTable(string databaseName, string tableName);

        /// <summary>
        /// Creates a tables with the specified <paramref name="columns"/>
        /// </summary>
        /// <param name="tableName">The name of the new table.</param>
        /// <param name="columns">The columns of the new table.</param>
        /// <param name="primaryKeyConstraintName">Empty if there are no primary key columns.</param>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName);

        /// <summary>
        /// Drops a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropTable(string tableName);

        /// <summary>
        /// Adds columns to an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddColumn(string tableName, Column column);

        /// <summary>
        /// Renames an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenameTable(string oldName, string newName);

        /// <summary>
        /// Renames a column of an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenameColumn(string tableName, string oldName, string newName);

        /// <summary>
        /// Removes a column from an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropColumn(string tableName, string columnName);

        /// <summary>
        /// Changes the data type of a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AlterColumn(string tableName, Column column);

        /// <summary>
        /// Adds an index to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName);

        /// <summary>
        /// Drops an index from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropIndex(string tableName, string indexName);

        /// <summary>
        /// Adds a foreign key constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName);

        /// <summary>
        /// Drops a foreign key constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropForeignKey(string tableName, string constraintName);

        /// <summary>
        /// Adds a primary key constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName);

        /// <summary>
        /// Renames the primary key.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName);

        /// <summary>
        /// Drops a primary key constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropPrimaryKey(string tableName, string constraintName);

        /// <summary>
        /// Adds an unique constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName);

        /// <summary>
        /// Drops a unique constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName);

        /// <summary>
        /// Drops the default value (constraint) from a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropDefault(string tableName, Column column);
    }

    internal static class ProviderExtensions
    {
        public static IEnumerable<SupportsAttribute> GetSupportsAttributes(this IProvider provider)
        {
            return provider.GetType().GetCustomAttributes(typeof(SupportsAttribute), true)
                .Cast<SupportsAttribute>()
                .OrderBy(a => a.DbType);
        }

        public static IEnumerable<UnsupportedMethod> GetUnsupportedMethods(this IProvider provider)
        {
            var unsupportedMethods = new List<UnsupportedMethod>();
            foreach (MethodInfo method in typeof(IProvider).GetMethods())
            {
                try
                {
                    method.Invoke(provider, GetDefaultParameters(method));
                }
                catch (TargetInvocationException x)
                {
                    NotSupportedException notSupportedException = x.InnerException as NotSupportedException;
                    if (notSupportedException != null)
                    {
                        unsupportedMethods.Add(new UnsupportedMethod(method.Name, notSupportedException.Message));
                    }
                    // other exception types are disregarded
                }
            }
            return unsupportedMethods;
        }

        private static object[] GetDefaultParameters(MethodInfo method)
        {
            var parameters = new List<object>();
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.ParameterType == typeof(string))
                {
                    parameters.Add(string.Empty);
                }
                else if (parameter.ParameterType == typeof(IEnumerable<CreatedColumn>))
                {
                    parameters.Add(Enumerable.Empty<CreatedColumn>());
                }
                else if (parameter.ParameterType == typeof(IEnumerable<Column>))
                {
                    parameters.Add(Enumerable.Empty<Column>());
                }
                else if (parameter.ParameterType == typeof(IEnumerable<ColumnReference>))
                {
                    parameters.Add(Enumerable.Empty<ColumnReference>());
                }
                else if (parameter.ParameterType == typeof(IEnumerable<string>))
                {
                    parameters.Add(Enumerable.Empty<string>());
                }
                else if (parameter.ParameterType == typeof(Column))
                {
                    parameters.Add(new Column(string.Empty, new DataType(0, 0, 0), false, null));
                }
                else
                {
                    try
                    {
                        parameters.Add(Activator.CreateInstance(parameter.ParameterType));
                    }
                    catch (MissingMethodException)
                    {
                        Debug.Fail(string.Format("Could not find default constructor for type: {0}", parameter.ParameterType));
                        throw;
                    }
                }
            }
            return parameters.ToArray();
        }
    }
}