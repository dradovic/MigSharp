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

        void IExistingTable.Rename(string newName)
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

        ITable IExistingTableWithAddedColumn.OfLength(int length)
        {
            var command = (AddColumnCommand)_command.Children.Last();
            command.Length = length;
            return this;
        }

        ITable IExistingTableWithAddedColumn.WithDefault<T>(T value)
        {
            return WithDefault(value, false);
        }

        ITable IExistingTableWithAddedColumn.WithDefault(string value)
        {
            return WithDefault(value, false);
        }

        ITable IExistingTableWithAddedColumn.WithTemporaryDefault<T>(T value)
        {
            return WithDefault(value, true);
        }

        ITable IExistingTableWithAddedColumn.WithTemporaryDefault(string value)
        {
            return WithDefault(value, true);
        }

        private ITable WithDefault(object value, bool dropThereafter)
        {
            var command = (AddColumnCommand)_command.Children.Last();
            command.DefaultValue = value;
            command.DropThereafter = dropThereafter;
            return this;
        }

        INewTable INewTable.WithPrimaryKeyColumn(string columnName, DbType type)
        {
            if (type == DbType.String) throw new ArgumentException(string.Format("Not all providers support '{0}' for primary key columns.", type)); // TODO: extract this to a generic validation where providers are asked what they support and what not

            _command.Add(new CreateColumnCommand(_command, columnName, type, false, true));
            return this;
        }

        INewTable INewTable.WithNullableColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, true, false));
            return this;
        }

        INewTable INewTable.OfLength(int length)
        {
            var command = (CreateColumnCommand)_command.Children.Last();
            command.Length = length;
            return this;
        }
    }
}