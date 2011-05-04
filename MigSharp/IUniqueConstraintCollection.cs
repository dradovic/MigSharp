using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of existing unique constraints of a table.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IUniqueConstraintCollection
    {
        /// <summary>
        /// Gets an unique constraint by name.
        /// </summary>
        IUniqueConstraint this[string constraintName] { get; }
    }
}