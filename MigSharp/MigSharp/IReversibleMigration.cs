namespace MigSharp
{
    public interface IReversibleMigration : IMigration
    {
        /// <summary>
        /// Undoes all changes from the <see cref="IMigration.Up"/> method.
        /// </summary>
        void Down(IDatabase db);
    }
}