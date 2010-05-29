namespace MigSharp
{
    public interface IExistingTableCollection
    {
        IExistingTable this[string name] { get; }
    }
}