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
        private readonly ICommand _command;
        private readonly ColumnCollection _columns;

        public IExistingColumnCollection Columns { get { return _columns; } }

        internal Table(ICommand command)
        {
            _command = command;
            _columns = new ColumnCollection(_command);
        }

        public void Rename(string newName)
        {
            _command.Add(new RenameCommand(newName));
        }

        public IAlteredTable AddColumn<T>(string name, DbType type, T defaultValue, AddColumnOptions options) where T : struct
        {
            _command.Add(new AddColumnCommand(name, type, false, defaultValue, options));
            return this;
        }

        public IAlteredTable AddNullableColumn(string name, DbType type)
        {
            _command.Add(new AddColumnCommand(name, type, true, null, AddColumnOptions.None));
            return this;
        }
    }
}