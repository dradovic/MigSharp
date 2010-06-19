namespace MigSharp
{
    /// <summary>
    /// Represents an undoable <see cref="IMigration"/>.
    /// </summary>
    public interface IUndoableMigration : IMigration
    {
        /// <summary>
        /// Undoes all changes from this migration.
        /// </summary>
        void Down(IDatabase db);
    }
}