namespace MigSharp
{
    public interface IDatabase
    {
        IExistingTableCollection Tables { get; }
        INewTable CreateTable(string tableName);
    }
}