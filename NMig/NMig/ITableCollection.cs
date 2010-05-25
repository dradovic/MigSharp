namespace NMig
{
    public interface ITableCollection
    {
        Table this[string name] { get; }
    }
}