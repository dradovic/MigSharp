namespace NMig
{
    public interface IColumnCollection
    {
        Column this[string name] { get; }
    }
}