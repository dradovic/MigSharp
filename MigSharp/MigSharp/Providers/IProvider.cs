using System.Collections.Generic;

namespace MigSharp.Providers
{
    public interface IProvider
    {
        IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns);
        IEnumerable<string> AddColumns(string tableName, IEnumerable<AddedColumn> columns);
        IEnumerable<string> RenameTable(string oldName, string newName);
        IEnumerable<string> RenameColumn(string tableName, string oldName, string newName);
        IEnumerable<string> DropDefaultConstraint(string tableName, string columnName);
    }
}