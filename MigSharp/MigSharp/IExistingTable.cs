namespace MigSharp
{
    public interface IExistingTable : IExistingTableBase
    {
        IExistingColumnCollection Columns { get; }

        void Rename(string newName);
    }
}