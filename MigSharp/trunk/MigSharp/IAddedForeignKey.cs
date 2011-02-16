namespace MigSharp
{
    /// <summary>
    /// Represents a foreign key constraint which is about to be added to a table.
    /// </summary>
    public interface IAddedForeignKey
    {
        /// <summary>
        /// Specifies the columns on which the foreign key is defined.
        /// </summary>
        /// <remarks>
        /// This method is not called 'On', as 'On' is a reserved language keyword (see FxCop rule "Identifiers should not match keywords").
        /// </remarks>
        IAddedForeignKey Through(string columnName, string referencedColumnName);
    }
}