namespace MigSharp
{
    /// <summary>
    /// Represents an unique constraint.
    /// </summary>
    public interface IUniqueConstraint
    {
        /// <summary>
        /// Drops the unique constraint.
        /// </summary>
        void Drop();
    }
}