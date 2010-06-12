using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IExistingColumnCollection
    {
        IExistingColumn this[string name] { get; }
    }
}