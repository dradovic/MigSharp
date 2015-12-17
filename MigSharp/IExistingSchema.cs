namespace MigSharp
{
    /// <summary>
    /// Represents a database schema. SQL Server only.
    /// </summary>
    public interface IExistingSchema
    {
        /// <summary>
        /// Gets existing tables within the schema.
        /// </summary>
        IExistingTableCollection Tables { get; }

        /// <summary>
        /// Creates a new table in this database schema.
        /// </summary>
        /// <param name="tableName">The name of the new table.</param>
        /// <param name="primaryKeyConstraintName">Optionally, the name of the primary key constraint.</param>
        ICreatedTable CreateTable(string tableName, string primaryKeyConstraintName);

        /// <summary>
        /// Drops the schema.
        /// </summary>
        void Drop();
    }

    /// <summary>
    /// Contains extension methods for the <see cref="IExistingSchema"/> interface.
    /// </summary>
    public static class ExistingSchemaExtensions
    {
        /// <summary>
        /// Creates a new table on the database with a default primary key constraint name.
        /// </summary>
        public static ICreatedTable CreateTable(this IExistingSchema schema, string tableName)
        {
            return schema.CreateTable(tableName, null);
        }
    }
}