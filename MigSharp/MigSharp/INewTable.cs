using System.Data;

namespace MigSharp
{
    public interface INewTable
    {
        INewTable WithPrimaryKeyColumn(string columnName, DbType type);
        INewTable WithNullableColumn(string columnName, DbType type);
    }
}