namespace MigSharp
{
    public interface IExistingColumnCollection
    {
        IExistingColumn this[string name] { get; }
    }
}