using System;
using System.Data;

using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    [Flags]
    public enum AddColumnOptions
    {
        None = 0,
        DropDefaultAfterCreation = 1,
    }

    internal class Table : IExistingTable, IAlteredTable
    {
        private readonly AlterTableCommand _alterTableCommand;
        private readonly ColumnCollection _columns;

        public IExistingColumnCollection Columns { get { return _columns; } }

        internal Table(AlterTableCommand alterTableCommand)
        {
            _alterTableCommand = alterTableCommand;
            _columns = new ColumnCollection(alterTableCommand);
        }

        public void Rename(string newName)
        {
            _alterTableCommand.Add(new RenameCommand(newName));
        }

        public IAlteredTable AddColumn<T>(string name, DbType type, T defaultValue, AddColumnOptions options) where T : struct
        {
            _alterTableCommand.Add(new AddColumnCommand(name, type, false, defaultValue, options));
            return this;
        }

        public IAlteredTable AddNullableColumn(string name, DbType type)
        {
            _alterTableCommand.Add(new AddColumnCommand(name, type, true, null, AddColumnOptions.None));
            return this;
        }
    }
}