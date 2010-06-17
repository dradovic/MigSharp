namespace MigSharp
{
    public interface IDatabase
    {
        IExistingTableCollection Tables { get; }
        ICreatedTable CreateTable(string tableName);
    }
}