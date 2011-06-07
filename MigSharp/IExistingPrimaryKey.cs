namespace MigSharp
{
    /// <summary>
    /// Represents a primary key constraint.
    /// </summary>
    public interface IExistingPrimaryKey
    {
        /// <summary>
        /// Drops the primary key constraint.
        /// </summary>
        void Drop();

        /// <summary>
        /// Renames the primary key constraint and all associated resources (e.g. Oracle maintains an index along with the primary key which is renamed, too).
        /// </summary>
        /// <param name="newName"></param>
        void Rename(string newName);
    }
}