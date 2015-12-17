using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of existing database schemas.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IExistingSchemaCollection
    {
        /// <summary>
        /// Gets a table by its name.
        /// </summary>
        IExistingSchema this[string name] { get; }
    }
}