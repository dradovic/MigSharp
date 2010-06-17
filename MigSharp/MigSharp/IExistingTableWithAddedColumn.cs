namespace MigSharp
{
    public interface IExistingTableWithAddedColumn : IExistingTableBase
    {
        IExistingTableBase OfLength(int length);
        IExistingTableBase WithDefault<T>(T value) where T : struct;
        IExistingTableBase WithDefault(string value);
        IExistingTableBase WithTemporaryDefault<T>(T value) where T : struct;
        IExistingTableBase WithTemporaryDefault(string value);
    }
}