using System;
using System.Data;

using NMig.Core;
using NMig.Core.Commands;

namespace NMig
{
    [Flags]
    public enum AddColumnOptions
    {
        None = 0,
        DropDefaultAfterCreation = 1,
    }

    public class Table
    {
        private readonly AlterTableCommand _alterTableCommand;
        private readonly ColumnCollection _columns;

        internal Table(AlterTableCommand alterTableCommand)
        {
            _alterTableCommand = alterTableCommand;
            _columns = new ColumnCollection(alterTableCommand);
        }

        public IColumnCollection Columns { get { return _columns; } }

        public void Rename(string newName)
        {
            _alterTableCommand.Add(new RenameCommand(newName));
        }

        public Table AddColumn<T>(string name, DbType type, T defaultValue, AddColumnOptions options) where T : struct
        {
            _alterTableCommand.Add(new AddColumnCommand(name, type, false, defaultValue, options));
            return this;
        }

        public Table AddNullableColumn(string name, DbType type)
        {
            _alterTableCommand.Add(new AddColumnCommand(name, type, true, null, AddColumnOptions.None));
            return this;
        }
    }
}