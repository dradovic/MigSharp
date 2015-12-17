namespace MigSharp
{
    /// <summary>
    /// Represents a foreign key constraint which is about to be added to a table.
    /// </summary>
    public interface IAddedForeignKey
    {
        /// <summary>
        /// Specifies the schema of the referenced table. If omitted, the schema of the referencing table is assumed. SQL Server only.
        /// </summary>
        IAddedForeignKey InSchema(string schemaName);

        /// <summary>
        /// Specifies the columns on which the foreign key is defined.
        /// </summary>
        /// <remarks>
        /// This method is not called 'On', as 'On' is a reserved language keyword (see FxCop rule "Identifiers should not match keywords").
        /// </remarks>
        IAddedForeignKey Through(string columnName, string referencedColumnName);

        /// <summary>
        /// Sets cascade as the delete action.
        /// </summary>
        /// /// <remarks>
        /// Corresponding rows will be deleted from the referencing table if the referenced row is deleted from the parent table.
        /// </remarks>
        IAddedForeignKey CascadeOnDelete();
    }
}