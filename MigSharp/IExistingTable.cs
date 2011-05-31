namespace MigSharp
{
    /// <summary>
    /// Represents a table that was created before.
    /// </summary>
    public interface IExistingTable : IExistingTableBase
    {
        /// <summary>
        /// Gets the columns of the table.
        /// </summary>
        IExistingColumnCollection Columns { get; }

        /// <summary>
        /// Gets the primary key constraint of the table.
        /// </summary>
        /// <param name="constraintName">Optionally, the name of the primary key constraint. If null or empty, the default name will be used.</param>
        IExistingPrimaryKey PrimaryKey(string constraintName);

        /// <summary>
        /// Gets the unique constraints of the table.
        /// </summary>
        IIndexesCollection Indexes { get; }

        /// <summary>
        /// Gets the unique constraints of the table.
        /// </summary>
        IUniqueConstraintCollection UniqueConstraints { get; }

        /// <summary>
        /// Gets the foreign key constraints of the table.
        /// </summary>
        IForeignKeyCollection ForeignKeys { get; }

        /// <summary>
        /// Renames the table.
        /// </summary>
        void Rename(string newName);

        /// <summary>
        /// Drops the table.
        /// </summary>
        void Drop();

        /// <summary>
        /// Adds a primary key constraint to the table.
        /// </summary>
        /// <param name="constraintName">Optionally, the primary key constraint name. If null or empty, a default name will be generated.</param>
        IAddedPrimaryKey AddPrimaryKey(string constraintName);

        /// <summary>
        /// Adds an index to the table.
        /// </summary>
        /// <param name="indexName">Optionally, the index name. If null or empty, a default name will be generated.</param>
        IAddedIndex AddIndex(string indexName);

        /// <summary>
        /// Adds a foreign key constraint to another table.
        /// </summary>
        /// <param name="referencedTableName">The name of the referenced table.</param>
        /// <param name="constraintName">Optionally, the name of the foreign key constraint. If null or empty, a default name will be generated.</param>
        IAddedForeignKey AddForeignKeyTo(string referencedTableName, string constraintName);

        /// <summary>
        /// Adds an unique constraint to the table.
        /// </summary>
        /// <param name="constraintName">Optionally, the name of the unique constraint. If null or empty, a default name will be generated.</param>
        IAddedUniqueConstraint AddUniqueConstraint(string constraintName);
    }

    /// <summary>
    /// Contains extension methods for the <see cref="IExistingTable"/> interface.
    /// </summary>
    public static class ExistingTableExtensions
    {
        /// <summary>
        /// Gets the primary key constraint of the table with the default name.
        /// </summary>
        public static IExistingPrimaryKey PrimaryKey(this IExistingTable table)
        {
            return table.PrimaryKey(DefaultObjectNameProvider.GetPrimaryKeyConstraintName(table.TableName, null));
        }

        /// <summary>
        /// Adds a primary key constraint to the table with the default name.
        /// </summary>
        public static IAddedPrimaryKey AddPrimaryKey(this IExistingTable table)
        {
            return table.AddPrimaryKey(null);
        }

        /// <summary>
        /// Adds an index to the table.
        /// </summary>
        public static IAddedIndex AddIndex(this IExistingTable table)
        {
            return table.AddIndex(null);
        }

        /// <summary>
        /// Adds a foreign key constraint to another table with the default name.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="referencedTableName">The name of the referenced table.</param>
        public static IAddedForeignKey AddForeignKeyTo(this IExistingTable table, string referencedTableName)
        {
            return table.AddForeignKeyTo(referencedTableName, null);
        }

        /// <summary>
        /// Adds an unique constraint to the table with the default name.
        /// </summary>
        public static IAddedUniqueConstraint AddUniqueConstraint(this IExistingTable table)
        {
            return table.AddUniqueConstraint(null);
        }

        /// <summary>
        /// Gets an unique constraint by the name of its first column.
        /// </summary>
        public static IUniqueConstraint UniqueConstraintOf(this IExistingTable table, string firstColumnName)
        {
            return table.UniqueConstraints[DefaultObjectNameProvider.GetUniqueConstraintName(table.TableName, firstColumnName, null)];
        }

        /// <summary>
        /// Gets an foreign key constraint by the name of its referenced table.
        /// </summary>
        public static IForeignKey ForeignKeyTo(this IExistingTable table, string referencedTableName)
        {
            return table.ForeignKeys[DefaultObjectNameProvider.GetForeignKeyConstraintName(table.TableName, referencedTableName, null)];
        }
    }
}