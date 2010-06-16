using System.Data;

namespace MigSharp
{
    public interface ITable
    {
        IExistingTableWithAddedColumn AddColumn(string name, DbType type);
        IExistingTableWithAddedColumn AddNullableColumn(string name, DbType type);
    }

    public interface IExistingTable : ITable
    {
        IExistingColumnCollection Columns { get; }

        void Rename(string newName);
    }

    public interface IExistingTableWithAddedColumn : ITable
    {
        ITable OfLength(int length);
        ITable WithDefault<T>(T value) where T : struct;
        ITable WithDefault(string value);
        ITable WithTemporaryDefault<T>(T value) where T : struct;
        ITable WithTemporaryDefault(string value);
    }
}