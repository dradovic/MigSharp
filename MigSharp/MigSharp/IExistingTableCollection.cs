using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IExistingTableCollection
    {
        IExistingTable this[string name] { get; }
    }
}