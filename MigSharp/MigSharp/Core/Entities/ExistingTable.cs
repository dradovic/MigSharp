using System;
using System.Data;
using System.Linq;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingTable : IExistingTable, IExistingTableWithAddedColumn
    {
        private readonly AlterTableCommand _command;
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

        internal ExistingTable(AlterTableCommand command)
        {
            _command = command;
            _columns = new ColumnCollection(command);
        }

        void IExistingTable.Rename(string newName)
        {
            _command.Add(new RenameCommand(_command, newName));
        }

        public void Drop()
        {
            _command.Add(new DropCommand(_command));
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

        IExistingTableBase IExistingTableWithAddedColumn.OfLength(int length)
        {
            var command = (AddColumnCommand)_command.Children.Last();
            command.Length = length;
            return this;
        }

        IExistingTableBase IExistingTableWithAddedColumn.WithDefault<T>(T value)
        {
            return WithDefault(value, false);
        }

        IExistingTableBase IExistingTableWithAddedColumn.WithDefault(string value)
        {
            return WithDefault(value, false);
        }

        IExistingTableBase IExistingTableWithAddedColumn.WithTemporaryDefault<T>(T value)
        {
            return WithDefault(value, true);
        }

        IExistingTableBase IExistingTableWithAddedColumn.WithTemporaryDefault(string value)
        {
            return WithDefault(value, true);
        }

        private IExistingTableBase WithDefault(object value, bool dropThereafter)
        {
            var command = (AddColumnCommand)_command.Children.Last();
            command.DefaultValue = value;
            command.DropThereafter = dropThereafter;
            return this;
        }
    }
}