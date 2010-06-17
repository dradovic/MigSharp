namespace MigSharp
{
    public interface IDatabase
    {
        /// <summary>
        /// Gets existing tables.
        /// </summary>
        IExistingTableCollection Tables { get; }

        /// <summary>
        /// Creates a new table on the database.
        /// </summary>
        ICreatedTable CreateTable(string tableName);

        /// <summary>
        /// Executes a custom query.
        /// </summary>
        /// <param name="query">Custom SQL which must be understood by all providers that should be supported by this migration
        /// or alternatively use <see cref="ICustomQuery.IfUsing"/> to specify a provider explicitly.</param>
        ICustomQuery Execute(string query);
    }
}