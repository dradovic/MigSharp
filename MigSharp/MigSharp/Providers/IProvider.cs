using System.Collections.Generic;

namespace MigSharp.Providers
{
    public interface IProvider
    {
        string AddColumns(string tableName, IEnumerable<AddedColumn> columns);
        string RenameTable(string oldName, string newName);
        string RenameColumn(string oldName, string newName);
    }
}