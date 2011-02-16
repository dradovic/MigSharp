using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of existing foreign key constraints of a table.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IForeignKeyCollection
    {
        /// <summary>
        /// Gets an foreign key constraint by name.
        /// </summary>
        IForeignKey this[string constraintName] { get; }
    }
}