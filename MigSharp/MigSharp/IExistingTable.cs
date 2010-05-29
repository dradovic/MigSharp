using System.Data;

using MigSharp.Core;

namespace MigSharp
{
    public interface IExistingTable
    {
        IExistingColumnCollection Columns { get; }

        void Rename(string newName);
        IAlteredTable AddColumn<T>(string name, DbType type, T defaultValue, AddColumnOptions options) where T : struct;
        IAlteredTable AddNullableColumn(string name, DbType type);
    }
}