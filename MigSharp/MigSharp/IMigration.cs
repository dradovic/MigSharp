namespace MigSharp
{
    /// <summary>
    /// <para>
    /// The interface that needs to be implemented in order to define a migration.
    /// Additionally, the <see cref="MigrationExportAttribute"/> must be applied
    /// to a class implementing this interface in order to be recognized as a migration.
    /// </para>
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Applies the required changes to the provided <paramref name="db"/> for this migration.
        /// </summary>
        void Up(IDatabase db);

        /// <summary>
        /// Undoes all changes from the <see cref="Up"/> method.
        /// </summary>
        void Down(IDatabase db);
    }
}