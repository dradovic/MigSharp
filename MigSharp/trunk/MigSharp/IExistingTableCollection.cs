using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of existing tables.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IExistingTableCollection
    {
        /// <summary>
        /// Gets a table by its name.
        /// </summary>
        IExistingTable this[string name] { get; }
    }
}