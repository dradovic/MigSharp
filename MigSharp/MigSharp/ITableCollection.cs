namespace MigSharp
{
    // TODO: drop ITableCollection and use explicit AlterTable command instead? What about IColumnCollection? What about multiple calls AlterTable?
    public interface ITableCollection
    {
        Table this[string name] { get; }
    }
}