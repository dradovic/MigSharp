namespace MigSharp
{
    /// <summary>
    /// Represents an existing column.
    /// </summary>
    public interface IExistingColumn
    {
        /// <summary>
        /// Renames the column.
        /// </summary>
        void Rename(string newName);

        /// <summary>
        /// Removes the column from its table.
        /// </summary>
        void Drop();

        /// <summary>
        /// Drops the default constraint of the column.
        /// </summary>
        void DropDefaultConstraint();
    }
}