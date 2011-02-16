namespace MigSharp
{
    /// <summary>
    /// Represents a foreign key constraint.
    /// </summary>
    public interface IForeignKey
    {
        /// <summary>
        /// Drops the foreign key constraint.
        /// </summary>
        void Drop();
    }
}