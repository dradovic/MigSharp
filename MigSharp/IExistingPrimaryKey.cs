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
    }
}