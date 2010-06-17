using System.Data;

namespace MigSharp
{
    public interface IExistingTableBase
    {
        IExistingTableWithAddedColumn AddColumn(string name, DbType type);
        IExistingTableWithAddedColumn AddNullableColumn(string name, DbType type);
    }
}