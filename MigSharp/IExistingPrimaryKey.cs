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
        /// Renames the primary key constraint.
        /// </summary>
        /// <param name="newName"></param>
        void Rename(string newName);
    }
}