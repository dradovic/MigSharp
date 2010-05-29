using System;
using System.Data;

namespace MigSharp
{
    [Flags]
    public enum AddColumnOptions
    {
        None = 0,
        DropDefaultAfterCreation = 1,
    }

    public interface IAlteredTable
    {
        IAlteredTable AddColumn<T>(string name, DbType type, T defaultValue, AddColumnOptions options) where T : struct;
        IAlteredTable AddNullableColumn(string name, DbType type);
    }
}