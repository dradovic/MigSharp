namespace MigSharp
{
    /// <summary>
    /// Represents a table that was created before.
    /// </summary>
    public interface IExistingTable : IExistingTableBase
    {
        /// <summary>
        /// Get the columns of the table.
        /// </summary>
        IExistingColumnCollection Columns { get; }

        /// <summary>
        /// Renames the table.
        /// </summary>
        void Rename(string newName);

        /// <summary>
        /// Drops the table.
        /// </summary>
        void Drop();
    }
}