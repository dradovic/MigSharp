using System.Data;

namespace MigSharp
{
    public interface ICreatedTableBase
    {
        ICreatedTableWithAddedColumn WithPrimaryKeyColumn(string columnName, DbType type);
        ICreatedTableWithAddedColumn WithNullableColumn(string columnName, DbType type);
    }
}