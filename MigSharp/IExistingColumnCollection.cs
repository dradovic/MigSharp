using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of existing columns.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IExistingColumnCollection
    {
        /// <summary>
        /// Gets a column by its name.
        /// </summary>
        IExistingColumn this[string name] { get; }
    }
}