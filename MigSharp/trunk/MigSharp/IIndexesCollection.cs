using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of existing indexes of a table.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IIndexesCollection
    {
        /// <summary>
        /// Gets an index by name.
        /// </summary>
        IIndex this[string indexName] { get; }
    }
}