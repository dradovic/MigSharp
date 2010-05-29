namespace MigSharp
{
    public interface IColumnCollection
    {
        Column this[string name] { get; }
    }
}