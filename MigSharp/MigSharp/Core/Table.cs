using System.Data;

using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class Table : IExistingTable, IAlteredTable, INewTable
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
            _command.Add(new RenameCommand(_command, newName));
        }

        public IAlteredTable AddColumn<T>(string name, DbType type, T defaultValue, AddColumnOptions options) where T : struct
        {
            _command.Add(new AddColumnCommand(_command, name, type, false, defaultValue, options));
            return this;
        }

        public IAlteredTable AddNullableColumn(string name, DbType type)
        {
            _command.Add(new AddColumnCommand(_command, name, type, true, null, AddColumnOptions.None));
            return this;
        }

        public INewTable WithPrimaryKeyColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, false, true));
            return this;
        }

        public INewTable WithNullableColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, true, false));
            return this;
        }
    }
}