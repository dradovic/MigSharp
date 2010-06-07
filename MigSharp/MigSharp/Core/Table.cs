using System;
using System.Data;
using System.Linq;

using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class Table : IExistingTable, IExistingTableWithAddedColumn, INewTable
    {
        private readonly ICommand _command;
        private readonly ColumnCollection _columns;

        public IExistingColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    // this should never be the case as the fluent interface should not expose the Columns property
                    // for new tables
                    throw new InvalidOperationException("Cannot access existing Columns of a newly created table.");
                }
                return _columns;
            }
        }

        internal Table(ICommand command)
        {
            _command = command;

            AlterTableCommand alterTableCommand = command as AlterTableCommand;
            if (alterTableCommand != null)
            {
                _columns = new ColumnCollection(alterTableCommand);
            }
        }

        public void Rename(string newName)
        {
            _command.Add(new RenameCommand(_command, newName));
        }

        public IExistingTableWithAddedColumn AddColumn(string name, DbType type)
        {
            _command.Add(new AddColumnCommand(_command, name, type, false));
            return this;
        }

        public IExistingTableWithAddedColumn AddNullableColumn(string name, DbType type)
        {
            _command.Add(new AddColumnCommand(_command, name, type, true));
            return this;
        }

        public ITable WithDefault<T>(T value) where T : struct
        {
            return WithDefault(value, false);
        }

        public ITable WithDefault(string value)
        {
            return WithDefault(value, false);
        }

        public ITable WithTemporaryDefault<T>(T value) where T : struct
        {
            return WithDefault(value, true);
        }

        public ITable WithTemporaryDefault(string value)
        {
            return WithDefault(value, true);
        }

        private ITable WithDefault(object value, bool dropThereafter)
        {
            AddColumnCommand addColumnCommand = (AddColumnCommand)_command.Children.Last();
            addColumnCommand.DefaultValue = value;
            addColumnCommand.DropThereafter = dropThereafter;
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